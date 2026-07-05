namespace Module.Inventory.Features.Admin.StockItems.Shared.Models;

public abstract record class StockItemParameters
{
    public Guid StockLocationId { get; init; }
    public Guid VariantId { get; init; }
    public int CountOnHand { get; init; }
    public bool Backorderable { get; init; }
}
