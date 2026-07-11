using Module.Inventory.Services.Abstractions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Ordering.Features.Shared.Services;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Ordering.Features.Admin.Orders.Cancel;
/// <summary>Cancels an order across the system — voids pending payments via gateway, releases reserved inventory, and sends a cancellation notification to the customer.</summary>
public static partial class CancelOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger,
        IGatewayRegistry gatewayRegistry,
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

            var payments = await dbContext.Set<PaymentCapture>()
                .Where(p => p.OrderId == order.Id && p.State != PaymentRecordState.Void && p.State != PaymentRecordState.Failed)
                .ToListAsync(cancellationToken);

            foreach (var payment in payments)
            {
                var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
                if (gatewayResult.IsFailure)
                {
                    logger.LogWarning("Failed to find gateway for payment {PaymentId} for order {OrderId}: {Errors}",
                        payment.Id, order.Id, string.Join("; ", gatewayResult.Errors.Select(f => f.Message)));
                    continue;
                }
                var gw = gatewayResult.Value;

                var options = new GatewayOptions
                {
                    Email = order.Email ?? string.Empty,
                    Customer = order.Email ?? string.Empty,
                    OrderId = $"{order.Number}-{payment.Number}",
                    PaymentId = payment.Number,
                    IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                    StatementDescriptorSuffix = string.Empty,
                };
                var voidResult = await payment.VoidTransactionAsync(gw, options, cancellationToken: cancellationToken);
                if (voidResult.IsFailure)
                {
                    logger.LogWarning("Failed to void payment {PaymentId} for order {OrderId}: {Errors}",
                        payment.Id, order.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));
                }
            }

            if (wasPlaced)
            {
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
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
