using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Cancel;

/// <summary>Cancels a stock transfer; restores source stock if it was already InTransit.</summary>
public static partial class CancelStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Transitions the transfer to Canceled and restores stock if it was deducted.</summary>
        /// <param name="command">The command containing the transfer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Load: Find the transfer with items
            var transfer = await dbContext.Set<StockTransfer>()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            // Check: Transfer must exist
            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            // Compute: Determine if stock needs restoration
            var wasInTransit = transfer.State == TransferState.InTransit;

            // Update: Transition state to Canceled
            var cancelResult = transfer.Cancel();
            if (cancelResult.IsFailure) return cancelResult;

            // Restore: Return stock to source if it was previously deducted
            if (wasInTransit)
            {
                foreach (var item in transfer.TransferItems)
                {
                    // Load: Source stock item for this variant
                    var stockItem = await dbContext.Set<StockItem>()
                        .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                            && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                    if (stockItem is not null)
                    {
                        // Update: Restore deducted stock quantity
                        stockItem.CountOnHand += item.Quantity;
                        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

                        // Record: Create stock movement for cancellation audit trail
                        var movement = StockMovementMethod.Create(
                            stockItemId: stockItem.Id,
                            quantity: item.Quantity,
                            previousCountOnHand: stockItem.CountOnHand - item.Quantity,
                            originatorType: "Transfer",
                            originatorId: transfer.Id,
                            action: "transfer_canceled");

                        if (movement.IsSuccess)
                            dbContext.Set<StockMovement>().Add(movement.Value);
                    }
                }
            }

            // Await: Persist all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record cancellation for audit trail
            StockTransferLoggers.Canceled(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}