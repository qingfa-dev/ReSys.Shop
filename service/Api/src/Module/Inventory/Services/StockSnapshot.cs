namespace Module.Inventory.Services;

public sealed record StockSnapshot(
    int TotalOnHand,
    int TotalReserved,
    int TotalAvailable,
    bool Backorderable,
    IReadOnlyList<LocationStockSnapshot> Locations);

public sealed record LocationStockSnapshot(
    Guid StockLocationId,
    string LocationName,
    int CountOnHand,
    int ReservedCount,
    int AvailableCount,
    bool Active,
    bool Backorderable);