using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

/// <summary>Handles reservation of stock for a cart item with configurable TTL.</summary>
public static partial class ReserveCartStock
{
    public sealed record Command(Request Request, string CartToken) : ICommand<Response>;

    public sealed class CommandHandler(IStockChecker stockChecker)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the reserve cart stock command.</summary>
        /// <param name="command">The command containing variant, quantity, and cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the reservation details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Reserve: Create cart-scoped stock reservation via StockChecker
            var result = await stockChecker.ReserveForCartAsync(
                command.Request.VariantId,
                command.Request.Quantity,
                command.Request.StockLocationId!.Value,
                command.CartToken,
                command.Request.TtlMinutes,
                cancellationToken);

            // Guard: Return failure if reservation could not be created
            if (result.IsFailure) return result.Errors;

            // Map: Return reservation details
            var r = result.Value;
            return new Response
            {
                Id = r.Id,
                VariantId = r.VariantId,
                Quantity = r.Quantity,
                ExpiresAtUtc = r.ExpiresAtUtc!.Value,
                State = r.State.ToString()
            };
        }
    }
}
