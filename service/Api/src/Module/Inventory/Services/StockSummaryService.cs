using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;
using Module.Inventory.Services.Models;

namespace Module.Inventory.Services;

/// <summary>Computes per-variant stock summaries across all active locations, including on-hand, reserved, and available quantities.</summary>
public class StockSummaryService(IApplicationDbContext dbContext) : IStockSummaryService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Aggregates stock data across all active locations, producing a per-variant summary with location breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of variant stock summaries with on-hand, reserved, and available counts per location.</returns>
    public async Task<List<VariantStockSummary>> GetStockSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Load: Fetch all stock items with their locations
        var stockItems = await _dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
            .ToListAsync(cancellationToken);

        // Load: Fetch all active reservations grouped by variant and location
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
            .GroupBy(r => new { r.VariantId, r.StockLocationId })
            .Select(g => new { g.Key.VariantId, g.Key.StockLocationId, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(cancellationToken);

        // Aggregate: Build a lookup map of variant → location → reserved quantity
        var reservationMap = reservations
            .Where(r => r.StockLocationId.HasValue)
            .GroupBy(r => r.VariantId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved));

        // Aggregate: Group stock items by variant and compute totals with reservation accounting
        var grouped = stockItems
            .GroupBy(si => si.VariantId)
            .Select(g =>
            {
                var locationReservations = reservationMap.GetValueOrDefault(g.Key) ?? [];
                var locationBreakdown = g.Select(si =>
                {
                    var reserved = locationReservations.GetValueOrDefault(si.StockLocationId, 0);
                    var available = si.CountOnHand - reserved;
                    return new LocationStockInfo
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

                return new VariantStockSummary
                {
                    VariantId = g.Key,
                    TotalOnHand = totalOnHand,
                    TotalReserved = totalReserved,
                    TotalAvailable = totalAvailable >= 0 ? totalAvailable : 0,
                    LocationBreakdown = locationBreakdown
                };
            })
            .ToList();

        return grouped;
    }
}