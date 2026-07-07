using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Delete;

namespace Module.UnitTests.Promotions.Features.Admin.CouponCodes.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "DeleteCouponCode")]
public class DeleteCouponCodeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteCouponCode.CommandHandler _handler;

    public DeleteCouponCodeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(CouponCode).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteCouponCode.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should cancel coupon code successfully")]
    public async Task Handle_ShouldCancelCouponCode()
    {
        // Arrange
        var couponCode = new CouponCode
        {
            Id = Guid.NewGuid(),
            Code = "CANCELTEST",
            PromotionId = Guid.NewGuid(),
            State = CouponCodeState.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<CouponCode>().Add(couponCode);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCouponCode.Command(couponCode.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<CouponCode>().FirstAsync(c => c.Id == couponCode.Id, TestContext.Current.CancellationToken);
        saved.State.Should().Be(CouponCodeState.Canceled);
    }

    [Fact(DisplayName = "Handler: Should return not found when coupon code does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new DeleteCouponCode.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "CouponCode.NotFound");
    }

    [Fact(DisplayName = "Handler: Should return conflict when coupon code already canceled")]
    public async Task Handle_ShouldReturnConflict_WhenAlreadyCanceled()
    {
        // Arrange
        var couponCode = new CouponCode
        {
            Id = Guid.NewGuid(),
            Code = "ALREADYCANCELED",
            PromotionId = Guid.NewGuid(),
            State = CouponCodeState.Canceled,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<CouponCode>().Add(couponCode);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeleteCouponCode.Command(couponCode.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "CouponCode.AlreadyCanceled");
    }
}
