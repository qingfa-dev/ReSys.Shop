using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.BulkAdjust;

/// <summary>Handles bulk adjustment of stock item quantities.</summary>
public static partial class BulkAdjustStockItems
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the bulk adjust stock items command.</summary>
        /// <param name="command">The command containing adjustment data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            foreach (var item in request.Items)
            {
                var entity = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.Id == item.StockItemId, cancellationToken);

                if (entity is null)
                    return StockItemResult.Errors.NotFound(item.StockItemId);

                var previousCount = entity.CountOnHand;

                var affected = await dbContext.Set<StockItem>()
                    .Where(x => x.Id == item.StockItemId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.CountOnHand, x => x.CountOnHand + item.Quantity)
                        .SetProperty(x => x.ModifiedAtUtc, DateTimeOffset.UtcNow),
                    cancellationToken);

                if (affected == 0)
                    return StockItemResult.Errors.NotFound(item.StockItemId);

                var movementResult = StockMovementMapping.MapToDomain(
                    stockItemId: item.StockItemId,
                    quantity: item.Quantity,
                    previousCountOnHand: previousCount,
                    originatorType: "Adjustment",
                    reason: request.Reason);

                if (movementResult.IsSuccess)
                {
                    var movement = movementResult.Value;
                    movement.CreatedBy = currentUser.UserName;
                    dbContext.Set<StockMovement>().Add(movement);
                }

                StockItemLoggers.Adjusted(logger, CountOnHand: previousCount + item.Quantity, Id: item.StockItemId, ActionBy: currentUser.UserName);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.NoContent(StockItemResult.Success.BulkAdjusted);
        }
    }
}
