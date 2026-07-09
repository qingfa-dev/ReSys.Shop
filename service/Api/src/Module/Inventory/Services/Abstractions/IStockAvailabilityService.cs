namespace Module.Inventory.Services.Abstractions;

public interface IStockAvailabilityService
{
    Task<bool> IsAvailableAsync(Guid variantId, int quantity, Guid stockLocationId, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAnyLocationAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default);
}
