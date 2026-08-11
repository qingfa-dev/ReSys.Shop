using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.BulkAdjust;

/// <summary>Adjusts quantities for multiple stock items in a single operation, recording each adjustment as a movement.</summary>
public static partial class BulkAdjustStockItems
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Adjusts each stock item by the specified delta and records stock movements for audit.</summary>
        /// <param name="command">The command containing adjustment data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var request = command.Request;

            foreach (var item in request.Items)
            {
                var entity = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.Id == item.StockItemId, cancellationToken);

                if (entity is null)
                    return StockItemResult.Errors.NotFound(item.StockItemId);

                var previousCount = entity.CountOnHand;

                var adjustResult = entity.AdjustCountOnHand(item.Quantity, request.Reason);
                if (adjustResult.IsFailure)
                    return adjustResult;
                entity.ModifiedAtUtc = DateTimeOffset.UtcNow;

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