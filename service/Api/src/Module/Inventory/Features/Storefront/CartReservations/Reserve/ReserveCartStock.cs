using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

/// <summary>Handles reservation of stock for a cart item with configurable TTL.</summary>
public static partial class ReserveCartStock
{
    public sealed record Command(Request Request, string CartToken) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the reserve cart stock command.</summary>
        /// <param name="command">The command containing variant, quantity, and cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the reservation details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variantId = command.Request.VariantId;
            var quantity = command.Request.Quantity;
            var stockLocationId = command.Request.StockLocationId!.Value;
            var cartToken = command.CartToken;
            var ttlMinutes = command.Request.TtlMinutes;

            if (quantity <= 0)
                return StockReservationResult.Errors.QuantityZero;

            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

            if (stockItem is null)
                return StockReservationResult.Errors.InsufficientStock;

            var reserved = await dbContext.Set<StockReservation>()
                .Where(r => r.VariantId == variantId
                            && r.StockLocationId == stockLocationId
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                .SumAsync(r => r.Quantity, cancellationToken);

            var available = stockItem.CountOnHand - reserved;
            if (available < quantity)
                return StockReservationResult.Errors.InsufficientStock;

            var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes, cartToken: cartToken);
            if (result.IsFailure) return result.Errors;

            var reservation = result.Value;
            dbContext.Set<StockReservation>().Add(reservation);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = reservation.Id,
                VariantId = reservation.VariantId,
                Quantity = reservation.Quantity,
                ExpiresAtUtc = reservation.ExpiresAtUtc!.Value,
                State = reservation.State.ToString()
            };
        }
    }
}
