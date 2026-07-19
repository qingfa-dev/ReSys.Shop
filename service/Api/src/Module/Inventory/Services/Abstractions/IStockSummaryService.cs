using Module.Inventory.Services.Models;

namespace Module.Inventory.Services.Abstractions;

/// <summary>Computes per-variant stock summaries across all active locations.</summary>
public interface IStockSummaryService
{
    /// <summary>Returns a list of variant stock summaries with on-hand, reserved, and available counts.</summary>
    Task<List<VariantStockSummary>> GetStockSummaryAsync(CancellationToken cancellationToken = default);
}