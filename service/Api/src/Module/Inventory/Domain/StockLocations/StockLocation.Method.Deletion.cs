namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationMethod
{
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

    public static Result Restore(this StockLocation location)
    {
        if (!location.IsDeleted)
            return Result.Ok();

        location.IsDeleted = false;
        location.DeletedAtUtc = null;
        location.DeletedBy = null;

        return Result.Ok();
    }
}