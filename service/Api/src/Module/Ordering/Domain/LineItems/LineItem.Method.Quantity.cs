namespace Module.Ordering.Domain.LineItems;

public static partial class LineItemMethod
{
    #region Quantity Methods
    public static Result UpdateQuantity(this LineItem lineItem, int quantity)
    {
        // Validate: Quantity within [1, MaxQuantity] — domain invariant consistent with Create path
        if (quantity is < 1 or > LineItemConstant.MaxQuantity)
        {
            return LineItemResult.Errors.QuantityExceedsMax;
        }

        // Assign: Replace existing quantity; caller must verify Order edit window before invoking
        lineItem.Quantity = quantity;

        return lineItem.RecalculateTotal();
    }
    #endregion
}
