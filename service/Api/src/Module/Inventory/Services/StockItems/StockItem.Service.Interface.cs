using Module.Inventory.Services.Models;

namespace Module.Inventory.Services;

public interface IStockItemService
{
    Task<Result> AdjustStockAsync(Guid variantId, int delta, Guid stockLocationId, Guid orderId, CancellationToken ct = default);

    Task<Result<RestockResult>> RestockAsync(Guid stockItemId, int quantity, string? reference = null, string? reason = null, CancellationToken ct = default);
    Task<Result<bool>> IsAvailableAsync(Guid variantId, int quantity, Guid? stockLocationId = null, CancellationToken ct = default);
    Task<Result<StockSnapshot>> GetSnapshotForVariantAsync(Guid variantId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VariantStockAvailability>>> GetStockAvailabilityAsync(IEnumerable<Guid> variantIds, CancellationToken ct = default);
    Task<Result<List<VariantStockSummary>>> GetStockSummaryAsync(CancellationToken ct = default);
}
