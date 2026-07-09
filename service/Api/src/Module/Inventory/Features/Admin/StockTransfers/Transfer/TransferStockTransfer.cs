using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Transfer;

/// <summary>Handles execution of a stock transfer (Draft → InTransit).</summary>
public static partial class TransferStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the transfer stock transfer command.</summary>
        /// <param name="command">The command containing the transfer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var transfer = await dbContext.Set<StockTransfer>()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            foreach (var item in transfer.TransferItems)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                        && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                if (stockItem is null || stockItem.CountOnHand < item.Quantity)
                    return StockTransferResult.Failure.InsufficientStockAtSource;
            }

            var transitionResult = transfer.Transfer();
            if (transitionResult.IsFailure) return transitionResult;

            foreach (var item in transfer.TransferItems)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstAsync(si => si.VariantId == item.VariantId
                        && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                var previousCount = stockItem.CountOnHand;
                stockItem.CountOnHand -= item.Quantity;
                stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

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
