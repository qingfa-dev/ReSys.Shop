namespace Module.Promotions.Features.Admin.CouponCodes.Shared.Models;

/// <summary>Detail response for a coupon code.</summary>
public class CouponCodeDetailResponse : CouponCodeParameters
{
    /// <summary>Gets or sets the coupon code ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the coupon code state.</summary>
    public string State { get; init; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}

/// <summary>List item response for a coupon code.</summary>
public class CouponCodeListItemResponse : CouponCodeDetailResponse { }
