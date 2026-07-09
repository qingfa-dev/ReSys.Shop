namespace Module.Ordering.Domain.LineItems;

public static partial class LineItemMethod
{
    #region Factory Methods
    public static Result<LineItem> Create(
        Guid orderId,
        Guid variantId,
        int quantity,
        decimal price)
    {
        // Validate: Quantity within [1, MaxQuantity] — domain invariant prevents overflow and negative stock
        if (quantity is < 1 or > LineItemConstant.MaxQuantity)
        {
            return LineItemResult.Errors.QuantityExceedsMax;
        }

        // Validate: Non-negative price — prevents negative totals and accounting inconsistencies
        if (price < 0)
        {
            return LineItemResult.Errors.InvalidPrice;
        }

        // Create: LineItem snapshot with UTC timestamp for audit trail
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
}
