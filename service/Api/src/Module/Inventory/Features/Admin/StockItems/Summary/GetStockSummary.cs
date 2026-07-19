using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

/// <summary>Handles retrieval of consolidated per-variant stock summary across all locations.</summary>
public static partial class GetStockSummary
{
    public sealed record Query : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Executes the get stock summary query.</summary>
        /// <param name="request">The query (no parameters needed).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of per-variant stock summaries.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            // Load: Fetch all stock items with location details for computing availability
            var stockItems = await dbContext.Set<StockItem>()
                .Include(si => si.StockLocation)
                .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
                .ToListAsync(cancellationToken);

            // Load: Fetch active reservation totals grouped by variant and location
            var reservations = await dbContext.Set<StockReservation>()
                .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
                .GroupBy(r => new { r.VariantId, r.StockLocationId })
                .Select(g => new { g.Key.VariantId, g.Key.StockLocationId, Reserved = g.Sum(r => r.Quantity) })
                .ToListAsync(cancellationToken);

            // Aggregate: Build lookup map of variant → location → reserved quantity
            var reservationMap = reservations
                .Where(r => r.StockLocationId.HasValue)
                .GroupBy(r => r.VariantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved));

            // Compute: Group stock items by variant and compute totals with reservation accounting
            var grouped = stockItems
                .GroupBy(si => si.VariantId)
                .Select(g =>
                {
                    var locationReservations = reservationMap.GetValueOrDefault(g.Key) ?? [];
                    var locationBreakdown = g.Select(si =>
                    {
                        var reserved = locationReservations.GetValueOrDefault(si.StockLocationId, 0);
                        var available = si.CountOnHand - reserved;
                        return new LocationBreakdownItem
                        {
                            LocationId = si.StockLocationId,
                            LocationName = si.StockLocation?.Name ?? "Unknown",
                            CountOnHand = si.CountOnHand,
                            Reserved = reserved,
                            Available = available >= 0 ? available : 0,
                            IsLowStock = si.StockLocation != null && si.CountOnHand <= si.StockLocation.LowStockThreshold
                        };
                    }).ToList();

                    var totalOnHand = locationBreakdown.Sum(l => l.CountOnHand);
                    var totalReserved = locationBreakdown.Sum(l => l.Reserved);
                    var totalAvailable = locationBreakdown.Sum(l => l.Available);

                    return new Response
                    {
                        VariantId = g.Key,
                        TotalOnHand = totalOnHand,
                        TotalReserved = totalReserved,
                        TotalAvailable = totalAvailable >= 0 ? totalAvailable : 0,
                        LocationBreakdown = locationBreakdown
                    };
                })
                .ToList();

            return grouped.ToList();
        }
    }
}