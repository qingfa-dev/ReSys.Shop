using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    public sealed record Response : StockSummaryDetailResponse;
}
