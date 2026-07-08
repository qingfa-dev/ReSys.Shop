namespace Module.Ordering.Domain.LineItems;

public static class LineItemExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new LineItem with the specified order, variant, quantity, and price.
    /// </summary>
    /// <param name="orderId">The parent order identifier.</param>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="quantity">The quantity of items.</param>
    /// <param name="price">The unit price.</param>
    /// <returns>A successful result containing the new LineItem with recalculated totals.</returns>
    // @CAT-10 Contract: pre=quantity>0&&quantity<=MaxQuantity&&price>=0, post=entity.Id!=null&&entity.Total==(quantity*price)
    // @CAT-4 Enforce: Line item price is locked at creation; cannot be modified after creation
    public static Result<LineItem> Create(
        Guid orderId,
        Guid variantId,
        int quantity,
        decimal price)
    {
        // Validate: Quantity must be within allowed range
        if (quantity is < 1 or > LineItemConstant.MaxQuantity)
        {
            return LineItemResult.Errors.QuantityExceedsMax;
        }

        // Validate: Price must be non-negative
        if (price < 0)
        {
            return LineItemResult.Errors.InvalidPrice;
        }

        var lineItem = new LineItem
        {
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            Price = price,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        lineItem.RecalculateTotal();

        return lineItem;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the line item quantity and recalculates totals.
    /// </summary>
    /// <param name="lineItem">The line item to update.</param>
    /// <param name="quantity">The new quantity value.</param>
    /// <returns>A success result with the updated line item ID.</returns>
    public static Result UpdateQuantity(this LineItem lineItem, int quantity)
    {
        // Validate: Quantity must be within allowed range
        if (quantity is < 1 or > LineItemConstant.MaxQuantity)
        {
            return LineItemResult.Errors.QuantityExceedsMax;
        }

        lineItem.Quantity = quantity;

        return lineItem.RecalculateTotal();
    }

    /// <summary>
    /// Recalculates the line item total based on quantity, price, and adjustments.
    /// </summary>
    /// <param name="lineItem">The line item to recalculate.</param>
    /// <returns>A success result with the recalculated line item ID.</returns>
    // Compute: Total = (Quantity * Price) + AdjustmentTotal
    public static Result RecalculateTotal(this LineItem lineItem)
    {
        lineItem.Total = (lineItem.Quantity * lineItem.Price)
                       + lineItem.AdjustmentTotal;

        return Result.Ok(LineItemResult.Success.Recalculated(lineItem.Id));
    }
    #endregion

    #region Computed Properties
    /// <summary>
    /// Returns the final amount for the line item: Total plus adjustments.
    /// </summary>
    /// <param name="lineItem">The line item to compute for.</param>
    /// <returns>The final line item amount.</returns>
    // @CAT-5 Compute: Final amount = Total + AdjustmentTotal
    public static decimal FinalAmount(this LineItem lineItem)
    {
        return lineItem.Total + lineItem.AdjustmentTotal;
    }
    #endregion
}
