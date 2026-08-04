using Microsoft.EntityFrameworkCore;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services;

/// <summary>Calculates real-time stock availability snapshots per variant, accounting for active reservations across locations.</summary>
public sealed class StockAvailabilityCalculator(IApplicationDbContext dbContext) : IStockAvailabilityCalculator
{
    /// <summary>Builds a full stock snapshot for a single variant across all active locations.</summary>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A StockSnapshot with per-location and aggregate on-hand, reserved, and available counts.</returns>
    public async Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        // Load: Fetch all stock items for the variant with location details
        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.VariantId == variantId)
            .AsNoTracking()
            .ToListAsync(ct);

        // Load: Fetch active reservation totals grouped by location for this variant
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

        // Compute: Per-location available stock = on-hand minus active reservations
        var locations = stockItems
            .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
            .Select(si =>
            {
                var reserved = reservedMap.GetValueOrDefault(si.StockLocationId, 0);
                var available = si.CountOnHand - reserved;
                return new LocationStockSnapshot
                {
                    StockLocationId = si.StockLocationId,
                    LocationName = si.StockLocation!.Name,
                    CountOnHand = si.CountOnHand,
                    ReservedCount = reserved,
                    AvailableCount = Math.Max(available, 0),
                    Active = si.StockLocation.Active,
                    Backorderable = si.Backorderable
                };
            })
            .ToList();

        // Aggregate: Rolling up per-location data into variant-level totals
        var totalOnHand = locations.Sum(l => l.CountOnHand);
        var totalReserved = locations.Sum(l => l.ReservedCount);
        var totalAvailable = Math.Max(totalOnHand - totalReserved, 0);
        var backorderable = locations.Any(l => l.Backorderable);

        return new StockSnapshot
        {
            TotalOnHand = totalOnHand,
            TotalReserved = totalReserved,
            TotalAvailable = totalAvailable,
            Backorderable = backorderable,
            Locations = locations
        };
    }

    /// <summary>Returns a dictionary of variant ID to available stock count for a batch of variants.</summary>
    /// <param name="variantIds">The variant identifiers to query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary mapping variant ID to net available stock (on-hand minus reserved).</returns>
    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct)
    {
        var ids = variantIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, int>();

        var now = DateTimeOffset.UtcNow;

        // Load: Aggregate total on-hand stock per variant across all locations
        var onHand = await dbContext.Set<StockItem>()
            .Where(si => ids.Contains(si.VariantId))
            .GroupBy(si => si.VariantId)
            .Select(g => new { VariantId = g.Key, OnHand = g.Sum(si => si.CountOnHand) })
            .ToListAsync(ct);

        // Load: Aggregate total active reservations per variant across all locations
        var reserved = await dbContext.Set<StockReservation>()
            .Where(r => ids.Contains(r.VariantId)
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.VariantId)
            .Select(g => new { VariantId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var onHandMap = onHand.ToDictionary(x => x.VariantId, x => x.OnHand);
        var reservedMap = reserved.ToDictionary(x => x.VariantId, x => x.Reserved);

        // Compute: For each requested variant, available = on-hand - reserved (floor 0)
        return ids.ToDictionary(
            id => id,
            id => Math.Max(
                onHandMap.GetValueOrDefault(id, 0) - reservedMap.GetValueOrDefault(id, 0),
                0));
    }

    /// <summary>Returns whether each variant is backorderable across any active, non-deleted stock location.</summary>
    /// <param name="variantIds">The variant identifiers to query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary mapping variant ID to a backorderable flag.</returns>
    public async Task<IReadOnlyDictionary<Guid, bool>> GetBackorderableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct)
    {
        var ids = variantIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, bool>();

        var backorderable = await dbContext.Set<StockItem>()
            .Where(si => ids.Contains(si.VariantId)
                         && si.StockLocation != null
                         && !si.StockLocation.IsDeleted
                         && si.StockLocation.Active
                         && si.Backorderable)
            .Select(si => si.VariantId)
            .Distinct()
            .ToListAsync(ct);

        var backorderableSet = backorderable.ToHashSet();
        return ids.ToDictionary(id => id, id => backorderableSet.Contains(id));
    }
}