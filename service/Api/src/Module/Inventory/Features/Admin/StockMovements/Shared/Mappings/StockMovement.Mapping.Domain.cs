using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;

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
