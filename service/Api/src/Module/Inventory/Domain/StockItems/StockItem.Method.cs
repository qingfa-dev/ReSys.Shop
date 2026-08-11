namespace Module.Inventory.Domain.StockItems;

public static partial class StockItemMethod
{
    public static Result<StockItem> Create(
        Guid stockLocationId,
        Guid variantId,
        bool backorderable = StockItemConstant.Defaults.Backorderable,
        int countOnHand = StockItemConstant.Defaults.CountOnHand)
    {
        if (countOnHand < 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        var stockItem = new StockItem
        {
            Id = Guid.NewGuid(),
            StockLocationId = stockLocationId,
            VariantId = variantId,
            Backorderable = backorderable,
            CountOnHand = countOnHand,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return Result<StockItem>.Ok(stockItem);
    }
}