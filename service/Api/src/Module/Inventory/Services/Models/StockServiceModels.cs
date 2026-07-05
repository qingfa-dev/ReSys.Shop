namespace Module.Inventory.Services;

/// <summary>Result of a restock operation including backorder fulfillment details.</summary>
public class RestockResult
{
    public Guid StockItemId { get; set; }
    public int PreviousCountOnHand { get; set; }
    public int NewCountOnHand { get; set; }
    public int BackordersFulfilled { get; set; }
    public int PartiallyFulfilled { get; set; }
    public int RemainingQuantity { get; set; }
    public Guid MovementId { get; set; }
}

internal class BackorderFulfillmentResult
{
    public int TotalFulfilled { get; set; }
    public int FullyFulfilled { get; set; }
    public int PartiallyFulfilled { get; set; }
}

public class VariantStockSummary
{
    public Guid VariantId { get; set; }
    public int TotalOnHand { get; set; }
    public int TotalReserved { get; set; }
    public int TotalAvailable { get; set; }
    public List<LocationStockInfo> LocationBreakdown { get; set; } = [];
}

public class LocationStockInfo
{
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int CountOnHand { get; set; }
    public int Reserved { get; set; }
    public int Available { get; set; }
    public bool IsLowStock { get; set; }
}
