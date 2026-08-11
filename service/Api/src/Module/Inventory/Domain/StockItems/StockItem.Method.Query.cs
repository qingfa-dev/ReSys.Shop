namespace Module.Inventory.Domain.StockItems;

public static partial class StockItemMethod
{
    public static int ProcessBackorders(this StockItem stockItem, int quantity)
    {
        if (quantity <= 0) return 0;

        if (stockItem.CountOnHand >= quantity)
            return quantity;

        var backorderCapacity = stockItem.Backorderable ? quantity - stockItem.CountOnHand : 0;
        var filledOnHand = Math.Min(stockItem.CountOnHand, quantity);
        var remaining = quantity - filledOnHand - backorderCapacity;

        return remaining;
    }

    public static (int OnHand, int Backordered) FillStatus(this StockItem stockItem, int requestedQuantity)
    {
        if (stockItem.CountOnHand >= requestedQuantity)
            return (requestedQuantity, 0);

        var onHand = stockItem.CountOnHand;
        var backordered = stockItem.Backorderable ? requestedQuantity - onHand : 0;
        return (onHand, backordered);
    }
}