namespace Module.Inventory.Domain.StockLocations.StockItems;

public static partial class StockItemMethod
{
    public static Result Update(this StockItem stockItem,
        int? countOnHand = null,
        bool? backorderable = null)
    {
        if (countOnHand.HasValue)
        {
            if (countOnHand.Value < 0)
                return StockItemResult.Errors.NegativeCountOnHand;
            stockItem.CountOnHand = countOnHand.Value;
        }

        if (backorderable.HasValue)
            stockItem.Backorderable = backorderable.Value;

        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    public static Result AdjustCountOnHand(this StockItem stockItem, int quantity, string? reason = null)
    {
        var newCount = stockItem.CountOnHand + quantity;

        if (newCount < 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        stockItem.CountOnHand = newCount;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Adjusted);
    }

    public static Result SetBackorderable(this StockItem stockItem, bool backorderable)
    {
        stockItem.Backorderable = backorderable;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    public static Result Restock(this StockItem stockItem, int quantity)
    {
        if (quantity <= 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        stockItem.CountOnHand += quantity;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Restocked);
    }

    public static Result Pick(this StockItem stockItem, int quantity)
    {
        if (quantity <= 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        if (stockItem.CountOnHand < quantity)
        {
            return StockItemResult.Errors.InsufficientStock;
        }

        stockItem.CountOnHand -= quantity;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Picked);
    }
}
