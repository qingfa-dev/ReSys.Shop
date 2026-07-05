using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Cancel;

/// <summary>Handles cancellation of a stock transfer (Draft|InTransit → Canceled).</summary>
public static partial class CancelStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the cancel stock transfer command.</summary>
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

            var wasInTransit = transfer.State == TransferState.InTransit;
            var cancelResult = transfer.Cancel();
            if (cancelResult.IsFailure) return cancelResult;

            if (wasInTransit)
            {
                foreach (var item in transfer.TransferItems)
                {
                    var stockItem = await dbContext.Set<StockItem>()
                        .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                            && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                    if (stockItem is not null)
                    {
                        stockItem.CountOnHand += item.Quantity;
                        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            StockTransferLoggers.Canceled(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}
