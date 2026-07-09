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

    // Compute: FinalAmount = Total + AdjustmentTotal — covers cases where Total lags behind a pending adjustment
    public static decimal FinalAmount(this LineItem lineItem)
    {
        return lineItem.Total + lineItem.AdjustmentTotal;
    }
    #endregion
}
