using Shared.Application.Contracts.Inventory;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Ordering.Features.Shared.Services;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Admin.Orders.Cancel;
/// <summary>Cancels an order across the system — voids pending payments via MediatR, releases reserved inventory, and sends a cancellation notification to the customer.</summary>
public static partial class CancelOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger,
        ISender sender,
        IStockQuantityService stockChecker) : ICommandHandler<Command, Response>
    {
        /// <summary>Voids payments, releases inventory for placed orders, persists the cancellation, and notifies the customer.</summary>
        /// <param name="command">The command containing the order ID and cancellation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The cancelled order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            var wasPlaced = order.Status == OrderStatus.Placed;
            Guid.TryParse(currentUser.UserId, out var userId);
            var result = order.Cancel(userId);
            if (result.IsFailure)
                return result.Errors;

            // Call: Void pending payments via Payment module — fire-and-forget on failure.
            var voidResult = await sender.Send(
                new Module.Payment.Features.Shared.Commands.VoidOrderPaymentsCommand(
                    order.Id, "Order cancelled by admin"),
                cancellationToken);
            if (voidResult.IsFailure)
            {
                // Log: Payment void failure is non-fatal — order is already cancelled.
                logger.LogWarning("Failed to void payments for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));
            }

            if (wasPlaced)
            {
                // Compensate: Release reserved inventory — order will not be fulfilled.
                foreach (var lineItem in order.LineItems)
                {
                    var orderInventory = new OrderInventoryService(order, lineItem, dbContext, stockChecker);
                    await orderInventory.RemoveAsync(lineItem.Quantity, cancellationToken);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await SendOrderCanceledNotificationAsync(order, cancellationToken);

            return order.MapToDetail<Response>();
        }

        private async Task SendOrderCanceledNotificationAsync(Order order, CancellationToken ct)
        {
            // Skip: No email on order — nothing to notify.
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            // Notify: Send cancellation email with order number and customer name.
            var message = NotificationMessage.Create(
                NotificationUseCase.OrderCancelled,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            // Suppress: Notification failure must not block order cancellation — best-effort only.
            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
