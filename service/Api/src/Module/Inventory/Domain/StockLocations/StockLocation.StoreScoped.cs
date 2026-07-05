namespace Module.Inventory.Domain.StockLocations;

// Invariant: Store association must not change after creation
public partial class StockLocation
{
    #region Store Scoping
    /// <summary>
    /// Gets or sets the store identifier for multi-store scoping.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Validates that the store association is not changed after creation.
    /// </summary>
    // Contract: pre=storeId is set only on creation, post=storeId is immutable
    // Enforce: Store association must not change for persisted locations
    public static Result ValidateStoreNotChanged(StockLocation location, Guid? newStoreId)
    {
        if (location.StoreId.HasValue && newStoreId.HasValue && location.StoreId != newStoreId)
        {
            return StockLocationResult.Errors.CannotChangeStore;
        }

        return Result.Ok();
    }
    #endregion
}
