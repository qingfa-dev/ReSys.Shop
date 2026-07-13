namespace Module.Ordering.Domain.Adjustments;

// Eligibility: Idempotent eligibility toggle — ineligible adjustments are excluded from ComputeAdjustmentTotal()
public static partial class AdjustmentMethod
{
    #region Eligibility Methods
    // Contract: pre=adjustment!=null, post=adjustment.Eligible==false, idempotent
    public static Result MarkIneligible(this Adjustment adjustment)
    {
        // Validate: Early-return when already ineligible — idempotency avoids unnecessary audit event emission
        if (!adjustment.Eligible)
        {
            return Result.Ok();
        }

        // Assign: Set Eligible to false — excludes this adjustment from future total recalculation without data loss
        adjustment.Eligible = false;

        return Result.Ok();
    }

    // Contract: pre=adjustment!=null, post=adjustment.Eligible==true, idempotent
    public static Result MarkEligible(this Adjustment adjustment)
    {
        // Validate: Early-return when already eligible — idempotency avoids unnecessary audit event emission
        if (adjustment.Eligible)
        {
            return Result.Ok();
        }

        // Assign: Set Eligible to true — re-includes this adjustment in the next ComputeAdjustmentTotal() pass
        adjustment.Eligible = true;

        return Result.Ok();
    }
    #endregion
}