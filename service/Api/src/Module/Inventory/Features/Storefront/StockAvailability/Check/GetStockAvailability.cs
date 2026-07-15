using Microsoft.EntityFrameworkCore;

using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

/// <summary>Checks stock availability for a variant across locations, accounting for active reservations and cart-specific holds.</summary>
public static partial class GetStockAvailability
{
    public sealed record Query(Request Request) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStockAvailabilityCalculator calculator) : IQueryHandler<Query, Response>
    {
        /// <summary>Loads stock items and reservations, then computes per-location and cart-specific availability.</summary>
        /// <param name="request">The query containing the variant identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the availability information.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            var snapshot = await calculator.GetForVariantAsync(req.VariantId, cancellationToken);

            var cartReserved = 0;
            if (!string.IsNullOrEmpty(req.CartToken))
            {
                cartReserved = await dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == req.VariantId
                                && r.CartToken == req.CartToken
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                    .SumAsync(r => r.Quantity, cancellationToken);
            }

            var availableToCart = Math.Max(snapshot.TotalAvailable - cartReserved, 0);

            // EXCEPTION: availability aggregate — no single domain entity to map from
            return new Response
            {
                VariantId = req.VariantId,
                TotalOnHand = snapshot.TotalOnHand,
                TotalReserved = snapshot.TotalReserved,
                CartReserved = cartReserved,
                TotalAvailable = snapshot.TotalAvailable,
                AvailableToCart = availableToCart,
                LocationAvailability = snapshot.Locations.Select(l => new LocationAvailability
                {
                    StockLocationId = l.StockLocationId,
                    LocationName = l.LocationName,
                    CountOnHand = l.CountOnHand,
                    ReservedCount = l.ReservedCount,
                    AvailableCount = l.AvailableCount,
                    Backorderable = l.Backorderable,
                    Available = l.AvailableCount > 0
                }).ToList()
            };
        }
    }
}