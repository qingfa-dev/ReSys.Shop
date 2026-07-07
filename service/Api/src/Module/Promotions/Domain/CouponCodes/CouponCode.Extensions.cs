namespace Module.Promotions.Domain.CouponCodes;

public static class CouponCodeExtensions
{
    #region Factory Methods
    /// <summary>Creates a new coupon code linked to a promotion.</summary>
    /// <param name="code">The coupon code string.</param>
    /// <param name="promotionId">The identifier of the promotion this code belongs to.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created CouponCode on success.</returns>
    // Contract: pre=code is non-null and non-empty, post=entity.Id is not default, throws=none
    public static Result<CouponCode> Create(
        string code,
        Guid promotionId,
        Guid? id = null)
    {
        return new CouponCode
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            PromotionId = promotionId,
            State = CouponCodeConstant.Defaults.State,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>Redeems the coupon code against an order, transitioning state to Redeemed.</summary>
    /// <param name="couponCode">The coupon code to redeem.</param>
    /// <param name="orderId">The identifier of the order being redeemed against.</param>
    /// <returns>A Result indicating success or a state-transition failure.</returns>
    // @CAT-4 Enforce: code active, promotion eligible, not already redeemed
    public static Result Redeem(this CouponCode couponCode, Guid orderId)
    {
        if (couponCode.State == CouponCodeState.Redeemed)
        {
            return CouponCodeResult.Errors.AlreadyRedeemed;
        }

        if (couponCode.State == CouponCodeState.Expired)
        {
            return CouponCodeResult.Errors.Expired;
        }

        if (couponCode.State == CouponCodeState.Canceled)
        {
            return CouponCodeResult.Errors.Canceled;
        }

        couponCode.State = CouponCodeState.Redeemed;
        couponCode.OrderId = orderId;
        couponCode.RedeemedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(CouponCodeResult.Success.Redeemed);
    }

    /// <summary>Expires the coupon code, making it permanently unusable, unless already redeemed.</summary>
    /// <param name="couponCode">The coupon code to expire.</param>
    /// <returns>A Result indicating success or a state-transition failure.</returns>
    public static Result Expire(this CouponCode couponCode)
    {
        if (couponCode.State == CouponCodeState.Expired)
        {
            return CouponCodeResult.Errors.AlreadyExpired;
        }

        if (couponCode.State == CouponCodeState.Redeemed)
        {
            return CouponCodeResult.Errors.AlreadyRedeemed;
        }

        couponCode.State = CouponCodeState.Expired;

        return Result.Ok(CouponCodeResult.Success.Expired);
    }

    /// <summary>Cancels the coupon code, preventing future redemption, unless already redeemed.</summary>
    /// <param name="couponCode">The coupon code to cancel.</param>
    /// <returns>A Result indicating success or a state-transition failure.</returns>
    public static Result Cancel(this CouponCode couponCode)
    {
        if (couponCode.State == CouponCodeState.Canceled)
        {
            return CouponCodeResult.Errors.AlreadyCanceled;
        }

        if (couponCode.State == CouponCodeState.Redeemed)
        {
            return CouponCodeResult.Errors.AlreadyRedeemed;
        }

        couponCode.State = CouponCodeState.Canceled;

        return Result.Ok(CouponCodeResult.Success.Canceled);
    }

    /// <summary>Checks whether the coupon code is still available for redemption.</summary>
    /// <param name="couponCode">The coupon code to check.</param>
    /// <returns>True if the coupon is in Active state.</returns>
    // @CAT-5 Compute: Active && promotion eligible
    public static bool IsRedeemable(this CouponCode couponCode)
    {
        return couponCode.State == CouponCodeState.Active;
    }
    #endregion Methods
}