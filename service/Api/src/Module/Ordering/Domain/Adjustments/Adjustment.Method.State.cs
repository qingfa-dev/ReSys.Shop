namespace Module.Ordering.Domain.Adjustments;

// State: Idempotent state transition methods — only "open" adjustments participate in total recalculation
public static partial class AdjustmentMethod
{
    #region State Methods
    // Contract: pre=adjustment!=null, post=adjustment.State=="closed", throws=AlreadyClosed when already closed
    public static Result Close(this Adjustment adjustment)
    {
        // Validate: Prevent double-close — closed adjustments are immutable to preserve historical totals
        if (adjustment.State == "closed")
        {
            return AdjustmentResult.Errors.AlreadyClosed;
        }

        // Assign: Transition to "closed" — freezes the amount so subsequent recalculation ignores this adjustment
        adjustment.State = "closed";

        return Result.Ok(AdjustmentResult.Success.Closed);
    }

    // Contract: pre=adjustment!=null, post=adjustment.State=="open", throws=AlreadyOpen when already open
    public static Result Open(this Adjustment adjustment)
    {
        // Validate: Prevent double-open — guard ensures idempotent transitions
        if (adjustment.State == "open")
        {
            return AdjustmentResult.Errors.AlreadyOpen;
        }

        // Assign: Transition to "open" — re-includes this adjustment in the next recalculation pass
        adjustment.State = "open";

        return Result.Ok(AdjustmentResult.Success.Opened);
    }
    #endregion
}