using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public record Response : StockItemListItemResponse
    {
        public string LocationName { get; init; } = string.Empty;
        public int Threshold { get; init; }
        public LowStockStatus Status { get; init; } = LowStockStatus.Low;
    }
}