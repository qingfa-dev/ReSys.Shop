using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.CouponCodes;

namespace Module.Promotions.Domain.Promotions;
/// <summary>Represents a Promotion.</summary>

// @CAT-10 Invariant: UsageLimit > 0 when set; PerCustomerUsageLimit > 0 when set; StartsAtUtc <= ExpiresAtUtc; MatchPolicy All|Any; Kind CouponCode|Automatic
public sealed partial class Promotion : AggregateRoot, IAuditable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? UsageLimit { get; set; }
    public int? PerCustomerUsageLimit { get; set; }
    public DateTimeOffset? StartsAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public MatchPolicy MatchPolicy { get; set; } = PromotionConstant.Defaults.MatchPolicy;
    public PromotionKind Kind { get; set; } = PromotionConstant.Defaults.Kind;
    public bool Advertise { get; set; }
    public bool Active { get; set; } = PromotionConstant.Defaults.Active;
    public int Position { get; set; }
    public string? Path { get; set; }
    public Guid? PromotionCategoryId { get; set; }
    #endregion Properties

    #region Relationships
    public PromotionCategory? PromotionCategory { get; set; }
    public ICollection<PromotionRule> PromotionRules { get; set; } = [];
    public ICollection<PromotionAction> PromotionActions { get; set; } = [];
    public ICollection<CouponCode> CouponCodes { get; set; } = [];
    #endregion Relationships

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal Promotion() { }
    #endregion Constructor
}