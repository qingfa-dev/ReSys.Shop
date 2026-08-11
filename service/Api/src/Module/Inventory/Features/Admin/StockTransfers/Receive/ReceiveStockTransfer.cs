using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Receive;

/// <summary>Receives items at the destination location, transitioning a transfer from InTransit to Received.</summary>
public static partial class ReceiveStockTransfer
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Validates the transfer state, receives each item at the destination, and records movements.</summary>
        /// <param name="command">The command containing receive data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && transfer.State==InTransit, post=result!=null, throws=DbUpdateException
            // Load: Find the transfer with items
            var transfer = await dbContext.Set<StockTransfer>()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            // Check: Transfer must exist
            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            // Check: Transfer must be InTransit to receive
            if (transfer.State != TransferState.InTransit)
                return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.Received);

            // Parse: Extract received items from the request
            var receivedItems = command.Request.Items
                .Select(i => (i.VariantId, i.Quantity))
                .ToList();

            foreach (var (variantId, quantity) in receivedItems)
            {
                // Update: Mark item as received on the transfer
                var receiveResult = transfer.Receive(variantId, quantity);
                if (receiveResult.IsFailure) return receiveResult;

                // Load: Destination stock item for this variant
                var destStockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == variantId
                        && si.StockLocationId == transfer.DestinationLocationId, cancellationToken);

                // Check: Stock item must exist at destination
                if (destStockItem is null)
                    return StockTransferResult.Failure.DestinationStockItemNotFound(variantId);

                var previousCount = destStockItem.CountOnHand;

                // Update: Add received quantity to destination stock
                destStockItem.CountOnHand += quantity;
                destStockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

                // Record: Create stock movement record for audit trail
                var movementResult = StockMovementMethod.Create(
                    stockItemId: destStockItem.Id,
                    quantity: quantity,
                    previousCountOnHand: previousCount,
                    originatorType: "Transfer",
                    originatorId: transfer.Id,
                    reason: $"Received from transfer {transfer.Number}",
                    action: "transfer_in",
                    stockLocationId: transfer.DestinationLocationId);

                if (movementResult.IsSuccess)
                    dbContext.Set<StockMovement>().Add(movementResult.Value);
            }

            // Await: Persist all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record receipt for audit trail
            StockTransferLoggers.Received(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}