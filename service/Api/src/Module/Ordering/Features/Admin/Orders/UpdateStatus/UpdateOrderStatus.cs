using Shared.Application.Contracts.Inventory;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Shared.Services;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

/// <summary>Transitions an order to a new status (e.g., canceled) with side effects — inventory release and audit logging.</summary>
public static partial class UpdateOrderStatus
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        IStockQuantityService stockChecker)
        : ICommandHandler<Command>
    {
        /// <summary>Applies a status transition (currently only cancel) to an order with inventory release and logging.</summary>
        /// <param name="command">The command containing the order ID and target status.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the existing entity.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            var request = command.Request;

            // Update: Apply status transition.
            switch (request.Status)
            {
                case OrderStatus.Canceled when entity.Status != OrderStatus.Canceled:
                    var wasPlaced = entity.Status == OrderStatus.Placed;
                    entity.Status = OrderStatus.Canceled;
                    entity.CanceledAtUtc = DateTimeOffset.UtcNow;
                    entity.CanceledById = Guid.TryParse(currentUser.UserId, out var canceledBy) ? canceledBy : null;

                    if (wasPlaced)
                    {
                        foreach (var li in entity.LineItems)
                        {
                            var orderInventory = new OrderInventoryService(entity, li, dbContext, stockChecker);
                            await orderInventory.RemoveAsync(li.Quantity, cancellationToken);
                        }
                    }
                    break;
                case OrderStatus.Placed when entity.Status == OrderStatus.Draft:
                    entity.Status = OrderStatus.Placed;
                    entity.CompletedAtUtc = DateTimeOffset.UtcNow;
                    break;
                default:
                    return OrderResult.Errors.InvalidStatusTransition;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record cancellation in audit log.
            if (entity.Status == OrderStatus.Canceled)
                OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
