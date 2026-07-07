using FluentAssertions;
using Module.Promotions.Domain.CouponCodes;

namespace Module.UnitTests.Promotions.Domain.CouponCodes;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "CouponCodeExtensions")]
public class CouponCodeExtensionTests
{
    private static readonly Guid PromotionId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should set properties")]
    public void Create_ShouldSetProperties()
    {
        var result = CouponCodeExtensions.Create("SUMMER20", PromotionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("SUMMER20");
        result.Value.PromotionId.Should().Be(PromotionId);
        result.Value.State.Should().Be(CouponCodeState.Active);
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Redeem: Should transition to redeemed when active")]
    public void Redeem_ShouldTransitionToRedeemed_WhenActive()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;

        var redeemResult = coupon.Redeem(Guid.NewGuid());

        redeemResult.IsSuccess.Should().BeTrue();
        coupon.State.Should().Be(CouponCodeState.Redeemed);
        coupon.OrderId.Should().NotBeNull();
        coupon.RedeemedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Redeem: Should return failure when already redeemed")]
    public void Redeem_ShouldReturnFailure_WhenAlreadyRedeemed()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Redeem(Guid.NewGuid());

        var redeemResult = coupon.Redeem(Guid.NewGuid());

        redeemResult.IsSuccess.Should().BeFalse();
        redeemResult.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyRedeemed");
    }

    [Fact(DisplayName = "Redeem: Should return failure when expired")]
    public void Redeem_ShouldReturnFailure_WhenExpired()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Expire();

        var redeemResult = coupon.Redeem(Guid.NewGuid());

        redeemResult.IsSuccess.Should().BeFalse();
        redeemResult.Failures.Should().Contain(f => f.Code == "CouponCode.Expired");
    }

    [Fact(DisplayName = "Redeem: Should return failure when canceled")]
    public void Redeem_ShouldReturnFailure_WhenCanceled()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Cancel();

        var redeemResult = coupon.Redeem(Guid.NewGuid());

        redeemResult.IsSuccess.Should().BeFalse();
        redeemResult.Failures.Should().Contain(f => f.Code == "CouponCode.Canceled");
    }

    [Fact(DisplayName = "Expire: Should transition to expired when active")]
    public void Expire_ShouldTransitionToExpired_WhenActive()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;

        var expireResult = coupon.Expire();

        expireResult.IsSuccess.Should().BeTrue();
        coupon.State.Should().Be(CouponCodeState.Expired);
    }

    [Fact(DisplayName = "Expire: Should transition to expired when canceled")]
    public void Expire_ShouldTransitionToExpired_WhenCanceled()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Cancel();

        var expireResult = coupon.Expire();

        expireResult.IsSuccess.Should().BeTrue();
        coupon.State.Should().Be(CouponCodeState.Expired);
    }

    [Fact(DisplayName = "Expire: Should return failure when already expired")]
    public void Expire_ShouldReturnFailure_WhenAlreadyExpired()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Expire();

        var expireResult = coupon.Expire();

        expireResult.IsSuccess.Should().BeFalse();
        expireResult.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyExpired");
    }

    [Fact(DisplayName = "Expire: Should return failure when redeemed")]
    public void Expire_ShouldReturnFailure_WhenRedeemed()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Redeem(Guid.NewGuid());

        var expireResult = coupon.Expire();

        expireResult.IsSuccess.Should().BeFalse();
        expireResult.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyRedeemed");
    }

    [Fact(DisplayName = "Cancel: Should transition to canceled when active")]
    public void Cancel_ShouldTransitionToCanceled_WhenActive()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;

        var cancelResult = coupon.Cancel();

        cancelResult.IsSuccess.Should().BeTrue();
        coupon.State.Should().Be(CouponCodeState.Canceled);
    }

    [Fact(DisplayName = "Cancel: Should transition to canceled when expired")]
    public void Cancel_ShouldTransitionToCanceled_WhenExpired()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Expire();

        var cancelResult = coupon.Cancel();

        cancelResult.IsSuccess.Should().BeTrue();
        coupon.State.Should().Be(CouponCodeState.Canceled);
    }

    [Fact(DisplayName = "Cancel: Should return failure when already canceled")]
    public void Cancel_ShouldReturnFailure_WhenAlreadyCanceled()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Cancel();

        var cancelResult = coupon.Cancel();

        cancelResult.IsSuccess.Should().BeFalse();
        cancelResult.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyCanceled");
    }

    [Fact(DisplayName = "Cancel: Should return failure when redeemed")]
    public void Cancel_ShouldReturnFailure_WhenRedeemed()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);
        var coupon = result.Value;
        coupon.Redeem(Guid.NewGuid());

        var cancelResult = coupon.Cancel();

        cancelResult.IsSuccess.Should().BeFalse();
        cancelResult.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyRedeemed");
    }

    [Fact(DisplayName = "IsRedeemable: Should return true when active")]
    public void IsRedeemable_ShouldReturnTrue_WhenActive()
    {
        var result = CouponCodeExtensions.Create("TEST", PromotionId);

        result.Value.IsRedeemable().Should().BeTrue();
    }

    [Fact(DisplayName = "IsRedeemable: Should return false when not active")]
    public void IsRedeemable_ShouldReturnFalse_WhenNotActive()
    {
        var activeResult = CouponCodeExtensions.Create("A", PromotionId);
        activeResult.IsSuccess.Should().BeTrue();
        activeResult.Value.IsRedeemable().Should().BeTrue();

        var redeemedResult = CouponCodeExtensions.Create("R", PromotionId);
        redeemedResult.IsSuccess.Should().BeTrue();
        redeemedResult.Value.Redeem(Guid.NewGuid());
        redeemedResult.Value.IsRedeemable().Should().BeFalse();

        var expiredResult = CouponCodeExtensions.Create("E", PromotionId);
        expiredResult.IsSuccess.Should().BeTrue();
        expiredResult.Value.Expire();
        expiredResult.Value.IsRedeemable().Should().BeFalse();

        var canceledResult = CouponCodeExtensions.Create("C", PromotionId);
        canceledResult.IsSuccess.Should().BeTrue();
        canceledResult.Value.Cancel();
        canceledResult.Value.IsRedeemable().Should().BeFalse();
    }
}
