using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Get.ById;

namespace Module.UnitTests.Promotions.Features.Admin.CouponCodes.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetCouponCodeById")]
public class GetCouponCodeByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCouponCodeById.QueryHandler _handler;

    public GetCouponCodeByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(CouponCode).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCouponCodeById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return coupon code when found")]
    public async Task Handle_ShouldReturnCouponCode_WhenFound()
    {
        // Arrange
        var couponCode = new CouponCode
        {
            Id = Guid.NewGuid(),
            Code = "TESTCODE",
            PromotionId = Guid.NewGuid(),
            State = CouponCodeState.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<CouponCode>().Add(couponCode);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetCouponCodeById.Query(couponCode.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(couponCode.Id);
        result.Value.Code.Should().Be("TESTCODE");
    }

    [Fact(DisplayName = "Handler: Should return not found when coupon code does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new GetCouponCodeById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "CouponCode.NotFound");
    }
}
