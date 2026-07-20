using Module.Inventory.Services.Models;

namespace Module.Inventory.Services.Abstractions;

/// <summary>Processes restock operations, fulfilling pending backorders before adding stock to on-hand.</summary>
public interface IStockRestockService
{
    /// <summary>Restocks a stock item, fulfilling backorders first in FIFO order.</summary>
    Task<Result<RestockResult>> RestockAsync(Guid stockItemId, int quantity, string? reference = null, string? reason = null, CancellationToken cancellationToken = default);
}