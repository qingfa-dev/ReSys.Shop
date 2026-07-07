using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Domain.OrderPromotions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Storefront.Cart;

namespace Module.UnitTests.Promotions.Features.Storefront.Cart;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "ApplyCoupon")]
public class ApplyCouponTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ApplyCoupon.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public ApplyCouponTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _handler = new ApplyCoupon.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Order> CreateDraftCart()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Number = "R123456789",
            Status = OrderStatus.Draft,
            Currency = "USD",
            ItemTotal = 150m,
            Total = 150m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private async Task<(Promotion promotion, CouponCode coupon)> CreateActiveCoupon()
    {
        var promotionId = Guid.NewGuid();
        var promotion = new Promotion
        {
            Id = promotionId,
            Name = "Test Promo",
            Active = true,
            Kind = PromotionKind.CouponCode,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Promotion>().Add(promotion);

        var coupon = new CouponCode
        {
            Id = Guid.NewGuid(),
            Code = "TESTCODE",
            PromotionId = promotionId,
            State = CouponCodeState.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<CouponCode>().Add(coupon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (promotion, coupon);
    }

    [Fact(DisplayName = "Handler: Should apply coupon when valid")]
    public async Task Handle_ShouldApplyCoupon_WhenValid()
    {
        // Arrange
        await CreateDraftCart();
        await CreateActiveCoupon();

        // Act
        var result = await _handler.Handle(
            new ApplyCoupon.Command(new ApplyCoupon.Request { Code = "TESTCODE" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var applied = await _dbContext.Set<OrderPromotion>().FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        applied.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return failure when coupon not found")]
    public async Task Handle_ShouldReturnFailure_WhenCouponNotFound()
    {
        // Arrange
        await CreateDraftCart();

        // Act
        var result = await _handler.Handle(
            new ApplyCoupon.Command(new ApplyCoupon.Request { Code = "INVALID" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return failure when user not authenticated")]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _handler.Handle(
            new ApplyCoupon.Command(new ApplyCoupon.Request { Code = "TESTCODE" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
