namespace Module.Inventory.Services.Abstractions;

/// <summary>Queries stock availability for variants at specific locations or across all locations.</summary>
public interface IStockAvailabilityService
{
    /// <summary>Checks whether a variant has sufficient un-reserved stock at a specific location.</summary>
    Task<bool> IsAvailableAsync(Guid variantId, int quantity, Guid stockLocationId, CancellationToken cancellationToken = default);
    /// <summary>Checks whether a variant has sufficient stock at any active location.</summary>
    Task<bool> IsAvailableAnyLocationAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default);
}