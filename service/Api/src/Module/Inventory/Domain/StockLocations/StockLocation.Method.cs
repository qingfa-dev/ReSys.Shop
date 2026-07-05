namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationMethod
{
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
}
