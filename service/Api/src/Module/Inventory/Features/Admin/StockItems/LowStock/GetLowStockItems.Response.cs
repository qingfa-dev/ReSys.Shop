using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed class Response : StockItemListItemResponse
    {
        public string LocationName { get; set; } = string.Empty;
        public int Threshold { get; init; }
        public string Status { get; set; } = "low";
    }
}
