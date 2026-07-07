using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Services.Abstractions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Events;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Module.Shipping.Domain.Shipments;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Ordering.Features.Storefront.Orders.Cancel;

    /// <summary>Handles CancelOrder feature.</summary>
    public static partial class CancelOrder
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStockChecker stockChecker,
        IPaymentGatewayActionProvider paymentGateway,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
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
                return OrderResult.Errors.NotFound(command.Id);

            // Check: Find the existing order scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Validate: Cannot cancel already canceled orders.
            if (entity.Status == OrderStatus.Canceled)
                return OrderResult.Errors.AlreadyCanceled;

            // Update: Transition order to Canceled.
            entity.Status = OrderStatus.Canceled;
            // Update: Modify entity properties.
            entity.CanceledAtUtc = DateTimeOffset.UtcNow;
            // Update: Modify entity properties.
            entity.CanceledById = currentUser.UserId is not null && Guid.TryParse(currentUser.UserId, out var canceledBy) ? canceledBy : null;

            // Void: Cancel associated payments via gateway.
            var payments = await dbContext.Set<PaymentDomain>()
                .Where(p => p.OrderId == entity.Id && p.State != PaymentState.Void && p.State != PaymentState.Failed)
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
                await payment.VoidTransactionAsync(paymentGateway, options, cancellationToken: cancellationToken);
            }

            // Cancel: Cancel associated shipments.
            var shipments = await dbContext.Set<Shipment>()
                .Where(s => s.OrderId == entity.Id)
                .ToListAsync(cancellationToken);

            foreach (var shipment in shipments)
            {
                shipment.Cancel();
            }

            // Restore: Restore stock for each line item on cancellation.
            if (entity.CompletedAtUtc.HasValue)
            {
                foreach (var lineItem in entity.LineItems)
                {
                    var orderInventory = new OrderInventory(entity, lineItem, dbContext, stockChecker);
                    await orderInventory.RemoveAsync(lineItem.Quantity, cancellationToken);
                }
            }

            // Raise: Order canceled domain event.
            entity.AddDomainEvent(new OrderCanceledEvent(
                entity.Id,
                entity.Number,
                entity.UserId!.Value,
                entity.Email ?? string.Empty,
                entity.CanceledAtUtc!.Value,
                currentUser.UserId));

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Success.
            OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(OrderResult.Success.Canceled(entity.Id));
        }
    }
}
