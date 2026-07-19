namespace Module.Ordering.Domain.LineItems;

public static partial class LineItemMethod
{
    #region Compute Methods
    // Compute: Total = (Quantity × Price) + AdjustmentTotal — adjustment is additive, not multiplicative
    public static Result RecalculateTotal(this LineItem lineItem)
    {
        lineItem.Total = (lineItem.Quantity * lineItem.Price)
                       + lineItem.AdjustmentTotal;

        return Result.Ok(LineItemResult.Success.Recalculated(lineItem.Id));
    }

    #endregion
}