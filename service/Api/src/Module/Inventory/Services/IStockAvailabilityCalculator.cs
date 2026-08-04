namespace Module.Inventory.Services;

/// <summary>Calculates real-time stock availability snapshots across locations for one or many variants.</summary>
public interface IStockAvailabilityCalculator
{
    /// <summary>Builds a full stock snapshot for a single variant across all active locations.</summary>
    Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct);
    /// <summary>Returns available stock counts for a batch of variants.</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct);
    /// <summary>Returns whether each variant is backorderable across any active location.</summary>
    Task<IReadOnlyDictionary<Guid, bool>> GetBackorderableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct);
}