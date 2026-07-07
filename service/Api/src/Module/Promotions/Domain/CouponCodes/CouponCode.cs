using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Domain.CouponCodes;
/// <summary>Represents a Coupon Code.</summary>

// @CAT-10 Invariant: Code unique; State Active→Redeemed→Expired or →Canceled
public sealed partial class CouponCode : AggregateRoot, IAuditable
{
    #region Properties
    public string Code { get; set; } = string.Empty;
    public CouponCodeState State { get; set; } = CouponCodeConstant.Defaults.State;
    public Guid? OrderId { get; set; }
    public DateTimeOffset? RedeemedAtUtc { get; set; }
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
    internal CouponCode() { }
    #endregion Constructor
}