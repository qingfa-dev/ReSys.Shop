using Microsoft.EntityFrameworkCore;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services;

public sealed class StockAvailabilityCalculator(IApplicationDbContext dbContext) : IStockAvailabilityCalculator
{
    public async Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.VariantId == variantId)
            .AsNoTracking()
            .ToListAsync(ct);

        var reservedByLocation = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.StockLocationId)
            .Select(g => new { StockLocationId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var reservedMap = reservedByLocation
            .Where(r => r.StockLocationId.HasValue)
            .ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved);

        var locations = stockItems
            .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
            .Select(si =>
            {
                var reserved = reservedMap.GetValueOrDefault(si.StockLocationId, 0);
                var available = si.CountOnHand - reserved;
                return new LocationStockSnapshot(
                    si.StockLocationId,
                    si.StockLocation!.Name,
                    si.CountOnHand,
                    reserved,
                    Math.Max(available, 0),
                    si.StockLocation.Active,
                    si.Backorderable);
            })
            .ToList();

        var totalOnHand = locations.Sum(l => l.CountOnHand);
        var totalReserved = locations.Sum(l => l.ReservedCount);
        var totalAvailable = Math.Max(totalOnHand - totalReserved, 0);
        var backorderable = locations.Any(l => l.Backorderable);

        return new StockSnapshot(totalOnHand, totalReserved, totalAvailable, backorderable, locations);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct)
    {
        var ids = variantIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, int>();

        var now = DateTimeOffset.UtcNow;

        var onHand = await dbContext.Set<StockItem>()
            .Where(si => ids.Contains(si.VariantId))
            .GroupBy(si => si.VariantId)
            .Select(g => new { VariantId = g.Key, OnHand = g.Sum(si => si.CountOnHand) })
            .ToListAsync(ct);

        var reserved = await dbContext.Set<StockReservation>()
            .Where(r => ids.Contains(r.VariantId)
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.VariantId)
            .Select(g => new { VariantId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var onHandMap = onHand.ToDictionary(x => x.VariantId, x => x.OnHand);
        var reservedMap = reserved.ToDictionary(x => x.VariantId, x => x.Reserved);

        return ids.ToDictionary(
            id => id,
            id => Math.Max(
                onHandMap.GetValueOrDefault(id, 0) - reservedMap.GetValueOrDefault(id, 0),
                0));
    }
}