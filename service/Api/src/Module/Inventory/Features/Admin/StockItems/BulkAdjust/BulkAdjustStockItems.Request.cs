using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.BulkAdjust;

public static partial class BulkAdjustStockItems
{
    public record Request : StockItemRequest
    {
        public Guid StockItemId { get; init; }
        public int Quantity { get; init; }
        public string? Reason { get; init; }
    }
}
