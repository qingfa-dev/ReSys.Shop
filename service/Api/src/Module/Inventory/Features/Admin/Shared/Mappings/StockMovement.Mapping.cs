using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.Shared.Mappings;

public static partial class StockMovementMapping
{
    public static Result<StockMovement> MapToDomain(
        Guid stockItemId,
        int quantity,
        int? previousCountOnHand = null,
        string? originatorType = null,
        Guid? originatorId = null,
        string? reason = null)
    {
        return StockMovementMethod.Create(
            stockItemId: stockItemId,
            quantity: quantity,
            previousCountOnHand: previousCountOnHand,
            originatorType: originatorType,
            originatorId: originatorId,
            reason: reason);
    }
}

public static partial class StockMovementMapping
{
    // Map: Domain entity -> Detail response
    public static T MapToDetail<T>(this StockMovement entity) where T : StockMovementDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            StockItemId = entity.StockItemId,
            Quantity = entity.Quantity,
            PreviousCountOnHand = entity.PreviousCountOnHand,
            Action = entity.Action,
            Reason = entity.Reason,
            OriginatorType = entity.OriginatorType,
            OriginatorId = entity.OriginatorId,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    // Map: Domain entity -> List item response
    public static T MapToListItem<T>(this StockMovement entity) where T : StockMovementListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            StockItemId = entity.StockItemId,
            Quantity = entity.Quantity,
            PreviousCountOnHand = entity.PreviousCountOnHand,
            Action = entity.Action,
            Reason = entity.Reason,
            OriginatorType = entity.OriginatorType,
            OriginatorId = entity.OriginatorId,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
