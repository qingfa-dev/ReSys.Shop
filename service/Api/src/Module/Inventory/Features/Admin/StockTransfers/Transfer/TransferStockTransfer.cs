using Microsoft.EntityFrameworkCore;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
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

            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            var transitionResult = transfer.Transfer();
            if (transitionResult.IsFailure) return transitionResult;

            foreach (var item in transfer.TransferItems)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                        && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                if (stockItem is null)
                    return StockTransferResult.Failure.InsufficientStockAtSource;

                var previousCount = stockItem.CountOnHand;

                var affected = await dbContext.Set<StockItem>()
                    .Where(x => x.VariantId == item.VariantId
                        && x.StockLocationId == transfer.SourceLocationId
                        && x.CountOnHand >= item.Quantity)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.CountOnHand, x => x.CountOnHand - item.Quantity)
                        .SetProperty(x => x.ModifiedAtUtc, DateTimeOffset.UtcNow),
                    cancellationToken);

                if (affected == 0)
                    return StockTransferResult.Failure.InsufficientStockAtSource;

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

            await dbContext.SaveChangesAsync(cancellationToken);

            StockTransferLoggers.Transferred(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}
