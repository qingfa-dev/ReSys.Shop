namespace Module.Inventory.Services;

public sealed record VariantStockAvailability
{
    public Guid VariantId { get; init; }
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public bool Backorderable { get; init; }
    public IReadOnlyList<LocationStockSnapshot> Locations { get; init; } = [];
}
