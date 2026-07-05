using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

public static partial class StockItemMapping
{
    public static T MapToDetail<T>(this StockItem entity) where T : StockItemDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            StockLocationId = entity.StockLocationId,
            VariantId = entity.VariantId,
            CountOnHand = entity.CountOnHand,
            Backorderable = entity.Backorderable,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this StockItem entity) where T : StockItemListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            StockLocationId = entity.StockLocationId,
            VariantId = entity.VariantId,
            CountOnHand = entity.CountOnHand,
            Backorderable = entity.Backorderable,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }
}
