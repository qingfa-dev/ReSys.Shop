using Module.Inventory.Services.Abstractions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Shared.Services;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

    /// <summary>Handles UpdateOrderStatus feature.</summary>
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
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
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
                default:
                    return OrderResult.Errors.InvalidStatusTransition;
            }

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Success.
            if (entity.Status == OrderStatus.Canceled)
                OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
