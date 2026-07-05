namespace Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

public static class StockMovementMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new stock movement recording a quantity change for a stock item.
    /// </summary>
    /// <param name="stockItemId">The stock item identifier.</param>
    /// <param name="quantity">The quantity moved. Positive for received stock, negative for shipped stock. Must not be zero.</param>
    /// <param name="previousCountOnHand">The count-on-hand before the movement.</param>
    /// <param name="originatorType">The originator type: Order, Transfer, or Adjustment.</param>
    /// <param name="originatorId">The originator identifier.</param>
    /// <param name="reason">Optional reason for the movement.</param>
    /// <returns>A result containing the created stock movement.</returns>
    // Guard: Prevent zero-quantity stock movements — no business meaning
    public static Result<StockMovement> Create(
        Guid stockItemId,
        int quantity,
        int? previousCountOnHand = null,
        string? originatorType = null,
        Guid? originatorId = null,
        string? reason = null)
    {
        // Guard: Quantity must not be zero — no movement without change
        if (quantity == 0)
            return StockMovementResult.Errors.QuantityZero;

        if (originatorType is not null
            && originatorType != "Order"
            && originatorType != "Transfer"
            && originatorType != "Adjustment"
            && originatorType != "Restock")
            return StockMovementResult.Errors.InvalidOriginatorType;

        return Result<StockMovement>.Ok(new StockMovement
        {
            StockItemId = stockItemId,
            Quantity = quantity,
            PreviousCountOnHand = previousCountOnHand ?? 0,
            OriginatorType = originatorType,
            OriginatorId = originatorId,
            Reason = reason,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        });
    }
    #endregion
}
