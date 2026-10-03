using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.StockMovements.Get.Paged;

public static partial class GetPagedStockMovements
{
    public record Response : StockMovementListItemResponse;
}