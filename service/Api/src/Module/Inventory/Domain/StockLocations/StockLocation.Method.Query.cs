namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationMethod
{
    public static bool StocksItem(this StockLocation location, Guid variantId)
    {
        return location.StockItems?.Any(si => si.VariantId == variantId) ?? false;
    }

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
}
