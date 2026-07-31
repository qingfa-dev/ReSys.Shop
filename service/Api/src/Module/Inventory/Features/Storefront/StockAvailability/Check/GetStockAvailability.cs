using Module.Inventory.Services;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

/// <summary>Checks stock availability for a variant across locations, accounting for active reservations and cart-specific holds.</summary>
public static partial class GetStockAvailability
{
    public sealed record Query(Request Request) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IStockAvailabilityCalculator calculator)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads stock items and reservations, then computes per-location availability.</summary>
        /// <param name="request">The query containing the variant identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the availability information.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            // Compute: Fetch stock snapshot with per-location availability
            var snapshot = await calculator.GetForVariantAsync(req.VariantId, cancellationToken);

            var items = snapshot.Locations.Select(l => new Response
            {
                StockLocationId = l.StockLocationId,
                LocationName = l.LocationName,
                CountOnHand = l.CountOnHand,
                ReservedCount = l.ReservedCount,
                AvailableCount = l.AvailableCount,
                Backorderable = l.Backorderable,
                Available = l.AvailableCount > 0
            }).ToList();

            return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
        }
    }
}
