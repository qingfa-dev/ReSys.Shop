using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Admin.StockTransfers.Cancel;

/// <summary>Handles cancellation of a stock transfer (Draft|InTransit → Canceled).</summary>
public static partial class CancelStockTransfer
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IStockChecker stockChecker,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>Executes the cancel stock transfer command.</summary>
        /// <param name="command">The command containing the transfer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Cancel: Restore source stock if InTransit
            var result = await stockChecker.CancelTransferAsync(command.Id, cancellationToken);

            // Log: Transfer canceled
            if (result.IsSuccess)
                StockTransferLoggers.Canceled(logger, Id: command.Id);

            return result;
        }
    }
}
