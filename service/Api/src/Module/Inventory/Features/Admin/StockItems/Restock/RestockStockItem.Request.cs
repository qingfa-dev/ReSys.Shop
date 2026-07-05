namespace Module.Inventory.Features.Admin.StockItems.Restock;

public static partial class RestockStockItem
{
    public sealed record Request
    {
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Reason { get; set; }
    }
}
