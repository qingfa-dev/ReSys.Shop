namespace Module.Promotions.Features.Admin.CouponCodes.Shared.Models;

/// <summary>Abstract base class for coupon code-related parameters.</summary>
public abstract class CouponCodeParameters
{
    /// <summary>Gets or sets the coupon code string.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets or sets the parent promotion ID.</summary>
    public Guid PromotionId { get; init; }
}
