using Shared.Application.Domain.Concerns.Auditable;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

public static partial class StockItemMapping
{
    public static Result<StockItem> MapToDomain<T>(this T request) where T : StockItemRequest
    {
        var result = StockItemMethod.Create(
            stockLocationId: request.StockLocationId,
            variantId: request.VariantId,
            countOnHand: request.CountOnHand,
            backorderable: request.Backorderable);

        if (result.IsSuccess)
        {
            AuditableBehavior.Create(result.Value);
        }

        return result;
    }

    public static Result MapToDomain<T>(this T request, StockItem stockItem) where T : StockItemRequest
    {
        var result = stockItem.Update(
            countOnHand: request.CountOnHand,
            backorderable: request.Backorderable);

        if (result.IsSuccess)
        {
            AuditableBehavior.Touch(stockItem);
        }

        return result;
    }
}
