using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

public static partial class StockItemMapping
{
    // Map: Request -> Domain entity (create)
    public static Result<StockItem> MapToDomain<T>(this T request) where T : StockItemRequest
    {
        return StockItemExtensions.Create(
            stockLocationId: request.StockLocationId,
            variantId: request.VariantId,
            countOnHand: request.CountOnHand,
            backorderable: request.Backorderable);
    }

    // Map: Request -> Domain entity (update)
    public static Result MapToDomain<T>(this T request, StockItem stockItem) where T : StockItemRequest
    {
        return stockItem.Update(
            countOnHand: request.CountOnHand,
            backorderable: request.Backorderable);
    }
}
