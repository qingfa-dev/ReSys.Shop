using Module.Billing.Features.Shared.Commands;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Shipping.Domain.Shipments;

using Shared.Application.Domain.Orders;
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
        IStockReservationService stockReservation) : ICommandHandler<Command, Response>
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
                .Include(o => o.Shipments)
                .Include(o => o.PaymentCaptures)
                .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            var wasPlaced = order.Status == OrderStatus.Placed;
            Guid.TryParse(currentUser.UserId, out var userId);
            var result = order.Cancel(userId);
            if (result.IsFailure)
                return result.Errors;

            order.RecomputePaymentState();

            // Call: Void pending payments via Payment module — fire-and-forget on failure.
            // TODO(audit 2026-08-16): cross-module ISender — keep ISender (gateway + txn + idempotency
            // keys); or extract to Billing IPaymentProcessingService and inject. Not a navigation fit.
            var voidResult = await sender.Send(
                new VoidOrderPaymentsCommand
                {
                    OrderId = order.Id,
                    Reason = OrderConstant.CancelReasons.Admin
                },
                cancellationToken);
            if (voidResult.IsFailure)
            {
                // Log: Payment void failure is non-fatal — order is already cancelled.
                OrderLoggers.VoidPaymentsFailed(logger, order.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));
            }

            foreach (var shipment in order.Shipments)
            {
                var shipmentCancelResult = shipment.Cancel();
                if (shipmentCancelResult.IsFailure)
                    return shipmentCancelResult.Errors;
            }

            order.ShipmentState = ShipmentState.Canceled;

            // Release: Return consumed stock for previously placed orders.
            if (wasPlaced)
            {
                var returnResult = await stockReservation.ReturnConsumedForOrderAsync(order.Id, cancellationToken);
                if (returnResult.IsFailure)
                    return returnResult.Errors;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await SendOrderCanceledNotificationAsync(order, cancellationToken);

            return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.Canceled(order.Id));
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
                OrderLoggers.CancelNotificationFailed(logger, order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}