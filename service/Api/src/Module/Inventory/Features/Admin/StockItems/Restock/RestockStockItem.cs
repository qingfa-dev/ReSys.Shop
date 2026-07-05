using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Admin.StockItems.Restock;

/// <summary>Handles restocking a stock item with backorder fulfillment.</summary>
public static partial class RestockStockItem
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IStockChecker stockChecker)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the restock stock item command.</summary>
        /// <param name="command">The command containing restock data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the restock outcome and backorder details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Execute: Restock via StockChecker (handles backorder fulfillment internally)
            var result = await stockChecker.RestockAsync(
                command.Id,
                command.Request.Quantity,
                command.Request.Reference,
                command.Request.Reason,
                cancellationToken);

            return result.IsSuccess
                ? Result<Response>.Ok(new Response(result.Value))
                : result.Errors;
        }
    }
}
