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
    // NOTE: This overload sums raw CountOnHand and does NOT account for active reservations.
    // Callers that need reservation-aware validation should use IsAvailableWithReservations.
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

    /// <summary>
    /// Validates stock availability accounting for active reservations.
    /// </summary>
    /// <param name="stockItems">Stock items at active locations.</param>
    /// <param name="reserved">Total active reserved quantity for the variant.</param>
    /// <param name="quantity">Requested quantity.</param>
    public static bool IsAvailableWithReservations(IEnumerable<StockItem> stockItems, int reserved, int quantity)
    {
        if (quantity <= 0) return true;

        var totalOnHand = stockItems
            .Where(si => si.StockLocation?.Active != false)
            .Sum(si => si.CountOnHand);

        var available = totalOnHand - reserved;
        if (available >= quantity) return true;

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