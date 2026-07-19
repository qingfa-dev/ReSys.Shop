namespace Module.Inventory.Services;

// Invariant: TotalAvailable == Math.Max(TotalOnHand - TotalReserved, 0); TotalOnHand >= 0
/// <summary>Aggregated stock snapshot across all locations for a single variant.</summary>
public sealed record StockSnapshot
{
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public bool Backorderable { get; init; }
    public IReadOnlyList<LocationStockSnapshot> Locations { get; init; } = default!;
}

/// <summary>Per-location stock breakdown within a variant's stock snapshot.</summary>
public sealed record LocationStockSnapshot
{
    public Guid StockLocationId { get; init; }
    public string LocationName { get; init; } = default!;
    public int CountOnHand { get; init; }
    public int ReservedCount { get; init; }
    public int AvailableCount { get; init; }
    public bool Active { get; init; }
    public bool Backorderable { get; init; }
}