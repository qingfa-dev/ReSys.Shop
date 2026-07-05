using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

/// <summary>Handles retrieval of stock availability for a variant including reservation accounting.</summary>
public static partial class GetStockAvailability
{
    public sealed record Query(Guid VariantId, string? CartToken = null) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get stock availability query accounting for active reservations.</summary>
        /// <param name="request">The query containing the variant identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the availability information.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve all stock items for the given variant across locations.
            var stockItems = await dbContext.Set<StockItem>()
                .Include(x => x.StockLocation)
                .Where(x => x.VariantId == request.VariantId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Query: Get active reserved quantities per location.
            var now = DateTimeOffset.UtcNow;
            var reservedByLocation = await dbContext.Set<StockReservation>()
                .Where(r => r.VariantId == request.VariantId
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > now)
                .GroupBy(r => r.StockLocationId)
                .Select(g => new { StockLocationId = g.Key, Reserved = g.Sum(r => r.Quantity) })
                .ToListAsync(cancellationToken);

            var reservedMap = reservedByLocation
                .Where(r => r.StockLocationId.HasValue)
                .ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved);

            // Map: Build availability response across locations with reservation accounting.
            var availability = stockItems
                .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
                .Select(si =>
                {
                    var reserved = reservedMap.GetValueOrDefault(si.StockLocationId, 0);
                    var available = si.CountOnHand - reserved;
                    return new LocationAvailability
                    {
                        StockLocationId = si.StockLocationId,
                        LocationName = si.StockLocation!.Name,
                        CountOnHand = si.CountOnHand,
                        ReservedCount = reserved,
                        AvailableCount = available >= 0 ? available : 0,
                        Backorderable = si.Backorderable,
                        Available = available > 0
                    };
                })
                .ToList();

            var totalOnHand = stockItems
                .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
                .Sum(si => si.CountOnHand);

            var totalReserved = reservedByLocation.Sum(r => r.Reserved);
            var totalAvailable = totalOnHand - totalReserved;

            // Compute: Cart-specific reserved quantity
            var cartReserved = 0;
            if (!string.IsNullOrEmpty(request.CartToken))
            {
                cartReserved = await dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == request.VariantId
                                && r.CartToken == request.CartToken
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > now)
                    .SumAsync(r => r.Quantity, cancellationToken);
            }

            var availableToCart = totalAvailable - cartReserved;

            return new Response
            {
                VariantId = request.VariantId,
                TotalOnHand = totalOnHand,
                TotalReserved = totalReserved,
                CartReserved = cartReserved,
                TotalAvailable = totalAvailable >= 0 ? totalAvailable : 0,
                AvailableToCart = availableToCart >= 0 ? availableToCart : 0,
                LocationAvailability = availability
            };
        }
    }
}
