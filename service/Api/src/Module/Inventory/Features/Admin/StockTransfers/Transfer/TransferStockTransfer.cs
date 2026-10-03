using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Transfer;

/// <summary>Executes a stock transfer by deducting source stock and transitioning from Draft to InTransit.</summary>
public static partial class TransferStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Validates the transfer, deducts stock at source, records movements, and persists.</summary>
        /// <param name="command">The command containing the transfer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && transfer.State==Draft, post=result!=null, throws=DbUpdateException
            // Load: Find the transfer with items
            var transfer = await dbContext.Set<StockTransfer>()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            // Check: Transfer must exist
            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            // Update: Transition state from Draft to InTransit
            var transitionResult = transfer.Transfer();
            if (transitionResult.IsFailure) return transitionResult;

            foreach (var item in transfer.TransferItems)
            {
                // Load: Source stock item for this variant
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                        && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                // Check: Stock item must exist at source
                if (stockItem is null)
                    return StockTransferResult.Failure.InsufficientStockAtSource;

                var previousCount = stockItem.CountOnHand;

                // Check: Sufficient stock at source
                if (stockItem.CountOnHand < item.Quantity)
                    return StockTransferResult.Failure.InsufficientStockAtSource;

                // Update: Deduct stock at source location
                stockItem.CountOnHand -= item.Quantity;
                stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

                // Record: Create stock movement record for audit trail
                var movementResult = StockMovementMethod.Create(
                    stockItemId: stockItem.Id,
                    quantity: -item.Quantity,
                    previousCountOnHand: previousCount,
                    originatorType: "Transfer",
                    originatorId: transfer.Id,
                    reason: $"Transfer {transfer.Number} to {transfer.DestinationLocationId}",
                    action: "transfer_out",
                    stockLocationId: transfer.SourceLocationId);

                if (movementResult.IsSuccess)
                    dbContext.Set<StockMovement>().Add(movementResult.Value);
            }

            // Await: Persist all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record transfer execution for audit trail
            StockTransferLoggers.Transferred(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}