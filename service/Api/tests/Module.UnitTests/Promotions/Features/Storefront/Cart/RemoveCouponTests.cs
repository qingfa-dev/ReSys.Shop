using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.OrderPromotions;
using Module.Promotions.Features.Storefront.Cart;

namespace Module.UnitTests.Promotions.Features.Storefront.Cart;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "RemoveCoupon")]
public class RemoveCouponTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RemoveCoupon.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public RemoveCouponTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OrderPromotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
        _handler = new RemoveCoupon.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should remove coupons when valid")]
    public async Task Handle_ShouldRemoveCoupons_WhenValid()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Number = "R123",
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Order>().Add(order);

        _dbContext.Set<OrderPromotion>().Add(new OrderPromotion
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PromotionId = Guid.NewGuid()
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new RemoveCoupon.Command(),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var remaining = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when user not authenticated")]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _handler.Handle(
            new RemoveCoupon.Command(),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
