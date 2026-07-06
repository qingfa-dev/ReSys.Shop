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
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Find the stock item.
            var entity = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == request.StockItemId, cancellationToken);

            if (entity is null)
                return StockItemResult.Errors.NotFound(request.StockItemId);

            // Check: Record previous count for audit trail.
            var previousCount = entity.CountOnHand;

            // Update: Apply the quantity change.
            var result = entity.AdjustCountOnHand(request.Quantity, request.Reason);
            if (result.IsFailure)
                return result.Errors;

            var movementResult = StockMovementMapping.MapToDomain(
                stockItemId: entity.Id,
                quantity: request.Quantity,
                previousCountOnHand: previousCount,
                originatorType: "Adjustment",
                reason: request.Reason);

            if (movementResult.IsFailure)
                return movementResult.Errors;

            var movement = movementResult.Value;
            movement.CreatedBy = currentUser.UserName;
            // Persist: Record the stock movement.
            dbContext.Set<StockMovement>().Add(movement);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock item adjusted.
            StockItemLoggers.Adjusted(logger, CountOnHand: entity.CountOnHand, Id: entity.Id, ActionBy: currentUser.UserName);

            // Map: Return success result.
            return Result.NoContent(StockItemResult.Success.BulkAdjusted);
        }
    }
}
