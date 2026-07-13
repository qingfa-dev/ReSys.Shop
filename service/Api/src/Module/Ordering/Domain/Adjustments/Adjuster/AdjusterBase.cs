namespace Module.Ordering.Domain.Adjustments.Adjuster;

/// <summary>
/// Base class for adjustment calculators that update totals during recalculation.
/// </summary>
// Invariant: Adjustable and Totals are required and must not be null
// Contract: pre=adjustable!=null && totals!=null, post=totals recalculated based on subclass logic
public abstract partial class AdjusterBase
{
    protected IAdjustable Adjustable { get; }
    protected IDictionary<string, decimal> Totals { get; }

    protected AdjusterBase(IAdjustable adjustable, IDictionary<string, decimal> totals)
    {
        Adjustable = adjustable;
        Totals = totals;
    }

    // Compute: Subclass-defined adjustment calculation — mutates Totals dictionary in place
    public abstract void Update();
}