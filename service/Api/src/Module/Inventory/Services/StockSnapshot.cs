namespace Module.Inventory.Services;

public sealed record StockSnapshot
{
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public bool Backorderable { get; init; }
    public IReadOnlyList<LocationStockSnapshot> Locations { get; init; } = default!;
}

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