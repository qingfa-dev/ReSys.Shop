namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationMethod
{
    public static Result SetAsDefault(this StockLocation location)
    {
        if (location.Default)
            return Result.Ok(StockLocationResult.Success.SetAsDefault);

        location.Default = true;

        return Result.Ok(StockLocationResult.Success.SetAsDefault);
    }
}