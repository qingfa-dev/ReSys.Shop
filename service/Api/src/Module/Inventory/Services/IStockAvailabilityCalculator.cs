namespace Module.Inventory.Services;

public interface IStockAvailabilityCalculator
{
    Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct);
}
