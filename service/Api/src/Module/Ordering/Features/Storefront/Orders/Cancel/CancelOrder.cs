using Shared.Application.Contracts.Inventory;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Shared.Services;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Storefront.Orders.Cancel;

/// <summary>Cancels a customer's placed order with payment voiding via MediatR, inventory release, and email notification.</summary>
public static partial class CancelOrder
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStockQuantityService stockChecker,
        ISender sender,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        INotificationService notificationService)
        : ICommandHandler<Command>
    {
        /// <summary>Voids payments, releases inventory for placed orders, persists cancellation, and notifies the customer.</summary>
        /// <param name="command">The command containing the order ID to cancel.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Failure.UserNotAuthenticated;

            // Check: Find the existing order scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return OrderResult.Failure.NotFound(command.Id);

            // Validate: Cannot cancel already canceled orders.
            if (entity.Status == OrderStatus.Canceled)
                return OrderResult.Failure.AlreadyCanceled;

            var wasPlaced = entity.Status == OrderStatus.Placed && entity.CompletedAtUtc.HasValue;

            entity.Status = OrderStatus.Canceled;
            // Update: Record cancellation timestamp.
            entity.CanceledAtUtc = DateTimeOffset.UtcNow;
            // Update: Record cancellation user identity.
            entity.CanceledById = currentUser.UserId is not null && Guid.TryParse(currentUser.UserId, out var canceledBy) ? canceledBy : null;

            // Call: Cancel associated payments via MediatR.
            var voidResult = await sender.Send(
                new Module.Payment.Features.Shared.Commands.VoidOrderPaymentsCommand(
                    entity.Id, "Order cancelled by customer"),
                cancellationToken);
            if (voidResult.IsFailure)
            {
                logger.LogWarning("Failed to void payments for order {OrderId}: {Errors}",
                    entity.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));
            }

            // Compensate: Release inventory back to stock for previously placed orders.
            if (wasPlaced)
            {
                foreach (var lineItem in entity.LineItems)
                {
                    var orderInventory = new OrderInventoryService(entity, lineItem, dbContext, stockChecker);
                    await orderInventory.RemoveAsync(lineItem.Quantity, cancellationToken);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order canceled notification to customer.
            await SendOrderCanceledNotificationAsync(entity, cancellationToken);

            // Log: Record cancellation in audit log.
            OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(OrderResult.Success.Canceled(entity.Id));
        }

        private async Task SendOrderCanceledNotificationAsync(Order order, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            var message = NotificationMessage.Create(
                NotificationUseCase.OrderCancelled,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            var result = await notificationService.SendAsync(message, ct);
            // Suppress: Notification failure does not roll back the cancellation.
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
