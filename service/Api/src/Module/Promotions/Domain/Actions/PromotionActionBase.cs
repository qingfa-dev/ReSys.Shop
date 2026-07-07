// Invariant: Preferences dictionary is never null; Type is always set
using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Promotion Action Base.</summary>

public abstract class PromotionActionBase : Entity, IAuditable
{
    #region Properties
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Preferences { get; set; } = [];
    #endregion Properties

    #region Relationships
    public Guid PromotionId { get; set; }
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    protected PromotionActionBase() { }
    #endregion Constructor

    #region Abstract Methods
    // @CAT-5 Compute: Enforce: Subclasses must implement eligibility check before performing action
    public abstract Result Perform(Dictionary<string, object> options);

    // Compute: Return the raw monetary amount from calculator before sign negation
    public abstract decimal ComputeAmount(object computable);

    // Enforce: Subclasses may implement revert logic for rollback
    public virtual Result Revert(Dictionary<string, object> options) => Result.Ok();
    #endregion Abstract Methods
}