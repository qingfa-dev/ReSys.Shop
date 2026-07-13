using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Domain.Stock;

/// <summary>
/// Validates stock availability across multiple stock items and locations.
/// </summary>
// Validate: Check whether requested quantity can be fulfilled across stock locations
public static class AvailabilityValidator
{
    // Validate: Check whether requested quantity can be fulfilled
    // Contract: pre=stockItems != null && quantity >= 0
    // post=result indicates whether stock is sufficient
    public static bool IsAvailable(IEnumerable<StockItem> stockItems, int quantity)
    {
        if (quantity <= 0)
            return true;

        var totalOnHand = stockItems
            .Where(si => si.StockLocation?.Active != false)
            .Sum(si => si.CountOnHand);

        if (totalOnHand >= quantity)
            return true;

        // Check: Whether any location can backorder
        var hasBackorderable = stockItems
            .Any(si => si.Backorderable && si.StockLocation?.Active != false);

        return hasBackorderable;
    }

    // Compute: Total available quantity across all active locations
    public static int TotalAvailable(IEnumerable<StockItem> stockItems)
    {
        return stockItems
            .Where(si => si.StockLocation?.Active != false)
            .Sum(si => si.CountOnHand);
    }
}