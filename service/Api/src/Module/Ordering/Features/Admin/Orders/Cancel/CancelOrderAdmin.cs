using Module.Inventory.Services.Abstractions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Ordering.Features.Shared.Services;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Ordering.Features.Admin.Orders.Cancel;
/// <summary>Handles CancelOrderAdmin feature.</summary>
public static partial class CancelOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ILogger<CommandHandler> logger,
        IPaymentGatewayActionProvider paymentGateway,
        IStockQuantityService stockChecker) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
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

            var payments = await dbContext.Set<PaymentRecord>()
                .Where(p => p.OrderId == order.Id && p.State != PaymentRecordState.Void && p.State != PaymentRecordState.Failed)
                .ToListAsync(cancellationToken);

            foreach (var payment in payments)
            {
                var options = new GatewayOptions(payment)
                {
                    Email = order.Email ?? string.Empty,
                    StatementDescriptorSuffix = string.Empty,
                    Customer = order.Email ?? string.Empty,
                    CustomerId = currentUser.UserId,
                    Ip = currentUser.IpAddress,
                    OrderId = $"{order.Number}-{payment.Number}",
                    PaymentId = payment.Number,
                    IdempotencyKey = $"spree-{payment.Number}"
                };
                var voidResult = await payment.VoidTransactionAsync(paymentGateway, options, cancellationToken: cancellationToken);
                if (voidResult.IsFailure)
                {
                    logger.LogWarning("Failed to void payment {PaymentId} for order {OrderId}: {Errors}",
                        payment.Id, order.Id, string.Join("; ", voidResult.Errors.Select(f => f.Description)));
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
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Description)));
            }
        }
    }
}
