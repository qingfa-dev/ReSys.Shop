using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;
using Module.Promotions.Domain.Promotions;

// Invariant: Type is always set; Preferences is never null; CalculatorType is nullable for non-calculator actions
namespace Module.Promotions.Domain.PromotionActions;
/// <summary>Represents a Promotion Action.</summary>

public sealed partial class PromotionAction : Entity, IAuditable
{
    #region Properties
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Preferences { get; set; } = [];
    public string? CalculatorType { get; set; }
    #endregion Properties

    #region Relationships
    public Guid PromotionId { get; set; }
    public Promotion Promotion { get; set; } = null!;
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal PromotionAction() { }
    #endregion Constructor
}