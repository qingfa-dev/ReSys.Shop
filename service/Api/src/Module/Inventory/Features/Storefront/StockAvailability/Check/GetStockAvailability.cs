using Microsoft.EntityFrameworkCore;

using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

/// <summary>Checks stock availability for a variant across locations, accounting for active reservations and cart-specific holds.</summary>
public static partial class GetStockAvailability
{
    public sealed record Query(Guid VariantId, string? CartToken = null) : IQuery<Response>;

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
            var snapshot = await calculator.GetForVariantAsync(request.VariantId, cancellationToken);

            var cartReserved = 0;
            if (!string.IsNullOrEmpty(request.CartToken))
            {
                cartReserved = await dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == request.VariantId
                                && r.CartToken == request.CartToken
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                    .SumAsync(r => r.Quantity, cancellationToken);
            }

            var availableToCart = Math.Max(snapshot.TotalAvailable - cartReserved, 0);

            return new Response
            {
                VariantId = request.VariantId,
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