using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

using Module.Promotions.Domain.Promotions;

// Invariant: Type is always set; Preferences is never null; PromotionId must reference a valid Promotion
namespace Module.Promotions.Domain.PromotionRules;
/// <summary>Represents a Promotion Rule.</summary>

public sealed partial class PromotionRule : Entity, IAuditable
{
    #region Properties
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Preferences { get; set; } = [];
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
    internal PromotionRule() { }
    #endregion Constructor
}