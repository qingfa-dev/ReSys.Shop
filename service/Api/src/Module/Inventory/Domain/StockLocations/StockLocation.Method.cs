namespace Module.Inventory.Domain.StockLocations;

public static class StockLocationMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new stock location with the specified properties.
    /// </summary>
    /// <param name="name">The stock location name. Must not be empty.</param>
    /// <param name="active">Whether the location is active.</param>
    /// <param name="isDefault">Whether this is the default stock location.</param>
    /// <param name="countryId">The country identifier.</param>
    /// <param name="stateId">The state identifier.</param>
    /// <param name="presentation">The display name for the location.</param>
    /// <param name="code">A unique code for the location.</param>
    /// <param name="address1">First address line.</param>
    /// <param name="address2">Second address line.</param>
    /// <param name="city">The city.</param>
    /// <param name="postalCode">The postal code.</param>
    /// <param name="phone">The phone number.</param>
    /// <param name="backorderableDefault">Default backorderable setting for stock items.</param>
    /// <param name="propagateAllVariants">Whether to propagate all variants.</param>
    /// <param name="adminName">The admin display name.</param>
    /// <param name="position">The sort position.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A result containing the created stock location.</returns>
    // Contract: pre=!string.IsNullOrEmpty(name), post=location.Id != Guid.Empty
    public static Result<StockLocation> Create(
        string name,
        bool active = StockLocationConstant.Defaults.Active,
        bool isDefault = StockLocationConstant.Defaults.Default,
        Guid? countryId = null,
        Guid? stateId = null,
        string? presentation = null,
        string? code = null,
        string? address1 = null,
        string? address2 = null,
        string? city = null,
        string? postalCode = null,
        string? phone = null,
        bool backorderableDefault = StockLocationConstant.Defaults.BackorderableDefault,
        bool propagateAllVariants = StockLocationConstant.Defaults.PropagateAllVariants,
        string? adminName = null,
        int position = StockLocationConstant.Defaults.Position,
        Guid? id = null)
    {
        var location = new StockLocation
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Presentation = presentation,
            Code = code,
            Address1 = address1,
            Address2 = address2,
            City = city,
            PostalCode = postalCode,
            Phone = phone,
            CountryId = countryId,
            StateId = stateId,
            Active = active,
            Default = isDefault,
            BackorderableDefault = backorderableDefault,
            PropagateAllVariants = propagateAllVariants,
            AdminName = adminName,
            Position = position,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return location;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the properties of an existing stock location. Only non-null parameters are applied.
    /// </summary>
    /// <param name="location">The stock location to update.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result Update(this StockLocation location,
        string? name = null,
        string? presentation = null,
        string? code = null,
        string? address1 = null,
        string? address2 = null,
        string? city = null,
        string? postalCode = null,
        string? phone = null,
        Guid? countryId = null,
        Guid? stateId = null,
        bool? active = null,
        bool? isDefault = null,
        bool? backorderableDefault = null,
        bool? propagateAllVariants = null,
        string? adminName = null,
        int? position = null)
    {
        if (active == false && location.Default)
        {
            return StockLocationResult.Errors.CannotDeactivateDefault;
        }

        location.Name = name ?? location.Name;
        location.Presentation = presentation ?? location.Presentation;
        location.Code = code ?? location.Code;
        location.Address1 = address1 ?? location.Address1;
        location.Address2 = address2 ?? location.Address2;
        location.City = city ?? location.City;
        location.PostalCode = postalCode ?? location.PostalCode;
        location.Phone = phone ?? location.Phone;
        location.CountryId = countryId ?? location.CountryId;
        location.StateId = stateId ?? location.StateId;
        location.Active = active ?? location.Active;
        location.Default = isDefault ?? location.Default;
        location.BackorderableDefault = backorderableDefault ?? location.BackorderableDefault;
        location.PropagateAllVariants = propagateAllVariants ?? location.PropagateAllVariants;
        location.AdminName = adminName ?? location.AdminName;
        location.Position = position ?? location.Position;

        return Result.Ok();
    }

    /// <summary>
    /// Soft-deletes the stock location. Deactivates the location first if active.
    /// </summary>
    /// <param name="location">The stock location to soft-delete.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result SoftDelete(this StockLocation location)
    {
        if (location.IsDeleted)
            return Result.Ok();

        if (location.Active)
            return StockLocationResult.Errors.CannotDeleteActive;

        location.IsDeleted = true;
        location.DeletedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// Restores a previously soft-deleted stock location.
    /// </summary>
    /// <param name="location">The stock location to restore.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result Restore(this StockLocation location)
    {
        if (!location.IsDeleted)
            return Result.Ok();

        location.IsDeleted = false;
        location.DeletedAtUtc = null;
        location.DeletedBy = null;

        return Result.Ok();
    }

    /// <summary>
    /// Activates the stock location, making it available for use.
    /// </summary>
    /// <param name="location">The stock location to activate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result Activate(this StockLocation location)
    {
        if (location.Active)
            return Result.Ok(StockLocationResult.Success.Activated);

        location.Active = true;

        return Result.Ok(StockLocationResult.Success.Activated);
    }

    /// <summary>
    /// Deactivates the stock location. Cannot deactivate the default location.
    /// </summary>
    /// <param name="location">The stock location to deactivate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result Deactivate(this StockLocation location)
    {
        if (!location.Active)
            return Result.Ok(StockLocationResult.Success.Deactivated);

        if (location.Default)
            return StockLocationResult.Errors.CannotDeactivateDefault;

        location.Active = false;

        return Result.Ok(StockLocationResult.Success.Deactivated);
    }

    /// <summary>
    /// Checks whether the stock location stocks the specified variant.
    /// </summary>
    // Contract: pre=variantId != Guid.Empty
    public static bool StocksItem(this StockLocation location, Guid variantId)
    {
        return location.StockItems?.Any(si => si.VariantId == variantId) ?? false;
    }

    /// <summary>
    /// Returns fill status (on_hand, backordered) for a variant at this location.
    /// Ported from Spree::StockLocation#fill_status.
    /// </summary>
    // Compute: on_hand = min(count_on_hand, quantity); backordered = remainder if backorderable
    public static (int OnHand, int Backordered) FillStatus(this StockLocation location, Guid variantId, int quantity)
    {
        var stockItem = location.StockItems?.FirstOrDefault(si => si.VariantId == variantId);
        if (stockItem == null) return (0, 0);

        if (stockItem.CountOnHand >= quantity)
        {
            return (quantity, 0);
        }

        var onHand = stockItem.CountOnHand < 0 ? 0 : stockItem.CountOnHand;
        var backordered = stockItem.Backorderable ? quantity - onHand : 0;
        return (onHand, backordered);
    }

    /// <summary>
    /// Sets this stock location as the default location.
    /// </summary>
    /// <param name="location">The stock location to set as default.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Enforce: Only one default stock location allowed across the system
    public static Result SetAsDefault(this StockLocation location)
    {
        if (location.Default)
            return Result.Ok(StockLocationResult.Success.SetAsDefault);

        location.Default = true;

        return Result.Ok(StockLocationResult.Success.SetAsDefault);
    }
    #endregion
}
