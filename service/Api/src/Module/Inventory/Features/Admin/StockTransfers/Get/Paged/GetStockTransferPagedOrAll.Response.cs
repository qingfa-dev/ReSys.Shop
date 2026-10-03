using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.StockTransfers.Get.Paged;

public static partial class GetStockTransferPagedOrAll
{
    public record Response : StockTransferListItemResponse;
}