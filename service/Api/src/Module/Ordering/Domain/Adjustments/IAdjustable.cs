namespace Module.Ordering.Domain.Adjustments;

/// <summary>
/// Defines an interface for entities that can have adjustments applied to them.
/// </summary>
// Contract: pre=adjustments!=null, post=list is immutable for external callers
// Boundary: Domain → Adjustments — implement on Order, LineItem, Shipment for polymorphic adjustment updates
public interface IAdjustable
{
    IReadOnlyList<Adjustment> Adjustments { get; }
}
