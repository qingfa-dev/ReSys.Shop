namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationMethod
{
    public static Result Activate(this StockLocation location)
    {
        if (location.Active)
            return Result.Ok(StockLocationResult.Success.Activated);

        location.Active = true;

        return Result.Ok(StockLocationResult.Success.Activated);
    }

    public static Result Deactivate(this StockLocation location)
    {
        if (!location.Active)
            return Result.Ok(StockLocationResult.Success.Deactivated);

        if (location.Default)
            return StockLocationResult.Errors.CannotDeactivateDefault;

        location.Active = false;

        return Result.Ok(StockLocationResult.Success.Deactivated);
    }

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
        if (active is not null)
        {
            var result = active.Value
                ? location.Activate()
                : location.Deactivate();

            if (result.IsFailure)
                return result;
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
        location.Default = isDefault ?? location.Default;
        location.BackorderableDefault = backorderableDefault ?? location.BackorderableDefault;
        location.PropagateAllVariants = propagateAllVariants ?? location.PropagateAllVariants;
        location.AdminName = adminName ?? location.AdminName;
        location.Position = position ?? location.Position;

        return Result.Ok();
    }
}