namespace Module.Ordering.Domain.Adjustments;

/// <summary>
/// Defines an interface for entities that can have adjustments applied to them.
/// </summary>
// Contract: pre=adjustments!=null, post=list is immutable for external callers
// Boundary: Domain → Adjustments — implement on Order, LineItem, Shipment for polymorphic adjustment updates
// AgentHint: Implementations must expose Adjustments via a backing List<Adjustment> for mutation;
//            cast to List<Adjustment> inside Adjusters when adding/removing entries
public interface IAdjustable
{
    /// <summary>Read-only snapshot of applied adjustments — use AdjustmentsUpdater for mutation.</summary>
    IReadOnlyList<Adjustment> Adjustments { get; }
}
