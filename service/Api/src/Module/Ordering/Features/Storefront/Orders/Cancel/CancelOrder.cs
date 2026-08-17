using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;

using Shared.Application.Domain.Orders;
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
        IStockReservationService stockReservation,
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
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the existing order scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Shipments)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            var wasPlaced = entity.Status == OrderStatus.Placed;

            var cancelResult = entity.Cancel(userId);
            if (cancelResult.IsFailure)
                return cancelResult.Errors;

            // Call: Cancel associated payments via MediatR.
            // TODO(audit 2026-08-16): cross-module ISender — keep ISender (gateway + txn + idempotency
            // keys); or extract to Billing IPaymentProcessingService and inject. Not a navigation fit.
            var voidResult = await sender.Send(
                new Billing.Features.Shared.Commands.VoidOrderPaymentsCommand
                {
                    OrderId = entity.Id,
                    Reason = OrderConstant.CancelReasons.Customer
                },
                cancellationToken);
            if (voidResult.IsFailure)
            {
                OrderLoggers.VoidPaymentsFailed(logger, entity.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));
            }

            foreach (var shipment in entity.Shipments)
            {
                var shipmentCancelResult = shipment.Cancel();
                if (shipmentCancelResult.IsFailure)
                    return shipmentCancelResult.Errors;
            }

            entity.ShipmentState = ShipmentState.Canceled;

            // Release: Return consumed stock for previously placed orders.
            if (wasPlaced)
            {
                var returnResult = await stockReservation.ReturnConsumedForOrderAsync(entity.Id, cancellationToken);
                if (returnResult.IsFailure)
                    return returnResult.Errors;
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
                OrderLoggers.CancelNotificationFailed(logger, order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}