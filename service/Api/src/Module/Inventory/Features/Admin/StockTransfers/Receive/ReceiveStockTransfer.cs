using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Features.Admin.StockTransfers.Receive;

/// <summary>Handles receiving items at destination for a stock transfer (InTransit → Received).</summary>
public static partial class ReceiveStockTransfer
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the receive stock transfer command.</summary>
        /// <param name="command">The command containing receive data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var transfer = await dbContext.Set<StockTransfer>()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (transfer is null)
                return StockTransferResult.Failure.NotFound;

            if (transfer.State != TransferState.InTransit)
                return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.Received);

            var receivedItems = command.Request.Items
                .Select(i => (i.VariantId, i.Quantity))
                .ToList();

            foreach (var (variantId, quantity) in receivedItems)
            {
                var receiveResult = transfer.Receive(variantId, quantity);
                if (receiveResult.IsFailure) return receiveResult;

                var destStockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == variantId
                        && si.StockLocationId == transfer.DestinationLocationId, cancellationToken);

                if (destStockItem is not null)
                {
                    var previousCount = destStockItem.CountOnHand;
                    destStockItem.CountOnHand += quantity;
                    destStockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

                    var movement = new StockMovement
                    {
                        Id = Guid.NewGuid(),
                        StockItemId = destStockItem.Id,
                        StockLocationId = transfer.DestinationLocationId,
                        Quantity = quantity,
                        PreviousCountOnHand = previousCount,
                        Action = "transfer_in",
                        OriginatorType = "Transfer",
                        OriginatorId = transfer.Id,
                        Reason = $"Received from transfer {transfer.Number}",
                        CreatedAtUtc = DateTimeOffset.UtcNow,
                        CreatedBy = "System"
                    };
                    dbContext.Set<StockMovement>().Add(movement);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            StockTransferLoggers.Received(logger, Id: command.Id);
            return Result.Ok();
        }
    }
}
