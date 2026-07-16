namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed record Request
    {
        public Guid? LocationId { get; init; }
        public int? Threshold { get; init; }
    }
}
