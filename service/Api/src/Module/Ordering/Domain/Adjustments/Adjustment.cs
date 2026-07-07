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
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DisplayAmount { get; set; } = string.Empty;
    public bool Eligible { get; set; }
    public bool Included { get; set; }
    public bool Mandatory { get; set; }
    public string State { get; set; } = AdjustmentConstant.Defaults.State;
    public Guid AdjustableId { get; set; }
    public string AdjustableType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal Adjustment() { }
    #endregion Constructor
}
