using Module.Inventory.Services.Abstractions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Shared.Services;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Ordering.Features.Storefront.Orders.Cancel;

    /// <summary>Handles CancelOrder feature.</summary>
    public static partial class CancelOrder
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStockQuantityService stockChecker,
        IPaymentGatewayActionProvider paymentGateway,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        INotificationService notificationService)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the existing order scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Validate: Cannot cancel already canceled orders.
            if (entity.Status == OrderStatus.Canceled)
                return OrderResult.Errors.AlreadyCanceled;

            var wasPlaced = entity.Status == OrderStatus.Placed && entity.CompletedAtUtc.HasValue;

            entity.Status = OrderStatus.Canceled;
            // Update: Modify entity properties.
            entity.CanceledAtUtc = DateTimeOffset.UtcNow;
            // Update: Modify entity properties.
            entity.CanceledById = currentUser.UserId is not null && Guid.TryParse(currentUser.UserId, out var canceledBy) ? canceledBy : null;

            // Void: Cancel associated payments via gateway.
            var payments = await dbContext.Set<PaymentRecord>()
                .Where(p => p.OrderId == entity.Id && p.State != PaymentRecordState.Void && p.State != PaymentRecordState.Failed)
                .ToListAsync(cancellationToken);

            foreach (var payment in payments)
            {
                var options = new GatewayOptions(payment)
                {
                    Email = entity.Email ?? string.Empty,
                    StatementDescriptorSuffix = string.Empty,
                    Customer = entity.Email ?? string.Empty,
                    CustomerId = currentUser.UserId,
                    Ip = currentUser.IpAddress,
                    OrderId = $"{entity.Number}-{payment.Number}",
                    PaymentId = payment.Number,
                    IdempotencyKey = $"spree-{payment.Number}"
                };
                var voidResult = await payment.VoidTransactionAsync(paymentGateway, options, cancellationToken: cancellationToken);
                if (voidResult.IsFailure)
                {
                    logger.LogWarning("Failed to void payment {PaymentId} for order {OrderId}: {Errors}",
                        payment.Id, entity.Id, string.Join("; ", voidResult.Errors.Select(f => f.Description)));
                }
            }

            if (wasPlaced)
            {
                foreach (var lineItem in entity.LineItems)
                {
                    var orderInventory = new OrderInventoryService(entity, lineItem, dbContext, stockChecker);
                    await orderInventory.RemoveAsync(lineItem.Quantity, cancellationToken);
                }
            }

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order canceled notification.
            await SendOrderCanceledNotificationAsync(entity, cancellationToken);

            // Log: Success.
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
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order canceled notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Description)));
            }
        }
    }
}
