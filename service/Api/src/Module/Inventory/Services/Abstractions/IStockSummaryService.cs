using Module.Inventory.Services.Models;

namespace Module.Inventory.Services.Abstractions;

public interface IStockSummaryService
{
    Task<List<VariantStockSummary>> GetStockSummaryAsync(CancellationToken cancellationToken = default);
}
