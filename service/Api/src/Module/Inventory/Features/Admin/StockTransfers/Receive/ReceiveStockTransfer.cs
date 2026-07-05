using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Admin.StockTransfers.Receive;

/// <summary>Handles receiving items at destination for a stock transfer (InTransit → Received).</summary>
public static partial class ReceiveStockTransfer
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IStockChecker stockChecker,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the receive stock transfer command.</summary>
        /// <param name="command">The command containing receive data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Map: Convert request items to tuples
            var receivedItems = command.Request.Items
                .Select(i => (i.VariantId, i.Quantity))
                .ToList();

            var result = await stockChecker.ReceiveTransferAsync(command.Id, receivedItems, cancellationToken);

            // Log: Transfer received
            if (result.IsSuccess)
                StockTransferLoggers.Received(logger, Id: command.Id);

            return result;
        }
    }
}
