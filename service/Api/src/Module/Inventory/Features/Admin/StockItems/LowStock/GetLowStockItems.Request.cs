using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed record Request : StockItemLowStockParameters;
}
