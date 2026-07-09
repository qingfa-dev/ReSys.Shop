using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Ordering.Domain.Adjustments;

/// <summary>
/// Represents an adjustment applied to an order or line item (e.g., discount, tax, shipping).
/// </summary>
    // @CAT-10 Invariant: Open adjustments auto-recalculate; Closed adjustments are locked; Amount can be positive (charge) or negative (credit)
public sealed partial class Adjustment : Entity, IAuditable
{
    #region Properties
    // Assign: Human-readable label, e.g. "10% Holiday Discount" — surfaced in UI and invoice line items
    public string Label { get; set; } = string.Empty;
    // Assign: Signed monetary value — positive charges the order, negative credits it; precision governed by AdjustmentConstant.Constraints
    public decimal Amount { get; set; }
    // Assign: Amount formatted for display as "F2" — invariant-culture string avoids locale mismatch in serialised payloads
    public string DisplayAmount { get; set; } = string.Empty;
    // Assign: When false, the adjustment is excluded from total recalculation; used for soft-disabling without data loss
    public bool Eligible { get; set; }
    // Assign: When true, the amount is already factored into the order total (e.g. tax); when false, it is additive (e.g. surcharge)
    public bool Included { get; set; }
    // Assign: When true, the adjustment cannot be removed by the customer-facing workflow (e.g. regulatory fees)
    public bool Mandatory { get; set; }
    // Assign: Finite-state lifecycle — "open" adjusts totals; "closed" freezes the amount for historical integrity
    public string State { get; set; } = AdjustmentConstant.Defaults.State;
    // Assign: FK to the entity being adjusted (Order, LineItem, or Shipment); polymorphic via AdjustableType discriminator
    public Guid AdjustableId { get; set; }
    // Assign: Discriminator for the AdjustableId — "Order", "LineItem", or "Shipment"
    public string AdjustableType { get; set; } = string.Empty;
    // Assign: FK to the business rule or promotion that created this adjustment (e.g. a ShippingRuleId, CouponId)
    public Guid SourceId { get; set; }
    // Assign: Discriminator for the SourceId — e.g. "Shipping" for auto-calculated shipping adjustments
    public string SourceType { get; set; } = string.Empty;
    // Assign: Owning order for namespace-scoped queries and aggregate boundary enforcement
    public Guid OrderId { get; set; }
    #endregion Properties

    #region Auditing
    // Assign: Immutable creation timestamp set once at factory — used for audit trail and time-range filters
    public DateTimeOffset CreatedAtUtc { get; set; }
    // Assign: Set on every state mutation — null until the first Close/Open/MarkEligible/MarkIneligible call
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    // Assign: Identity of the principal that created this adjustment — "System" for programmatic, user ID for manual
    public string? CreatedBy { get; set; }
    // Assign: Identity of the principal that last mutated this adjustment — null when not yet modified
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    // Boundary: Persistence → Domain — reserved for EF Core materialization; do not invoke from application code
    internal Adjustment() { }
    #endregion Constructor
}
