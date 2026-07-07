namespace Module.Ordering.Domain.Adjustments;

/// <summary>
/// Orchestrates adjustment recalculation for an adjustable entity.
/// </summary>
// Invariant: Adjustable must not be null; total is the sum of all adjustment amounts
// AgentHint: For fashion store v1, adjustments are manual only (admin-applied discounts).
public partial class AdjustmentsUpdater
{
    public IAdjustable Adjustable { get; }

    public AdjustmentsUpdater(IAdjustable adjustable)
    {
        Adjustable = adjustable;
    }

    public static decimal ComputeTotal(IAdjustable adjustable)
    {
        return new AdjustmentsUpdater(adjustable).ComputeAdjustmentTotal();
    }

    // Compute: Sum of all adjustment amounts
    public decimal ComputeAdjustmentTotal()
    {
        return Adjustable.Adjustments.Sum(a => a.Amount);
    }
}
