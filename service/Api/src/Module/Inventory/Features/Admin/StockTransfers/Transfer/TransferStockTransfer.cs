using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Admin.StockTransfers.Transfer;

/// <summary>Handles execution of a stock transfer (Draft → InTransit).</summary>
public static partial class TransferStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IStockChecker stockChecker,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the transfer stock transfer command.</summary>
        /// <param name="command">The command containing the transfer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Execute: Decrement source stock and transition to InTransit
            var result = await stockChecker.ExecuteTransferAsync(command.Id, cancellationToken);

            // Log: Transfer executed
            if (result.IsSuccess)
                StockTransferLoggers.Transferred(logger, Id: command.Id);

            return result;
        }
    }
}
