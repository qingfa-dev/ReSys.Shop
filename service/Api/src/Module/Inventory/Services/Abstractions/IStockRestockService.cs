using Module.Inventory.Services.Models;

namespace Module.Inventory.Services.Abstractions;

public interface IStockRestockService
{
    Task<Result<RestockResult>> RestockAsync(Guid stockItemId, int quantity, string? reference = null, string? reason = null, CancellationToken cancellationToken = default);
}