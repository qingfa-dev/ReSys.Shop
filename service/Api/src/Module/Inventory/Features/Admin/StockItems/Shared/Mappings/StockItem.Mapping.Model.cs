using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;
using Module.Inventory.Services;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

public static partial class StockItemMapping
{
    public static Result<StockItem> MapToDomain(this StockItemRequest request)
    {
        var result = StockItemMethod.Create(request.StockLocationId, request.VariantId, request.Backorderable, request.CountOnHand);
        return result;
    }

    public static Result MapToDomain(this StockItemRequest request, StockItem existing)
    {
        existing.CountOnHand = request.CountOnHand;
        existing.Backorderable = request.Backorderable;
        return Result.Ok();
    }

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

    public static T MapToRestockResponse<T>(
        this StockItem entity,
        int previousCountOnHand,
        int backordersFulfilled,
        int partiallyFulfilled,
        int remainingQuantity,
        Guid movementId) where T : RestockResult, new()
    {
        return new T
        {
            StockItemId = entity.Id,
            PreviousCountOnHand = previousCountOnHand,
            NewCountOnHand = entity.CountOnHand,
            BackordersFulfilled = backordersFulfilled,
            PartiallyFulfilled = partiallyFulfilled,
            RemainingQuantity = remainingQuantity,
            MovementId = movementId,
        };
    }

    public static LocationStockInfo MapToSummaryItem(this StockItem entity, int reserved, int available)
    {
        return new LocationStockInfo
        {
            LocationId = entity.StockLocationId,
            LocationName = entity.StockLocation?.Name ?? "Unknown",
            CountOnHand = entity.CountOnHand,
            Reserved = reserved,
            Available = available >= 0 ? available : 0,
            IsLowStock = entity.StockLocation?.LowStockThreshold is { } threshold && entity.CountOnHand <= threshold,
        };
    }
}
