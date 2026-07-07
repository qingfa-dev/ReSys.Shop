using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Create;

namespace Module.UnitTests.Promotions.Features.Admin.CouponCodes.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "CreateCouponCode")]
public class CreateCouponCodeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateCouponCode.CommandHandler _handler;

    public CreateCouponCodeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(CouponCode).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateCouponCode.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create coupon code successfully")]
    public async Task Handle_ShouldCreateCouponCode()
    {
        // Act
        var result = await _handler.Handle(
            new CreateCouponCode.Command(new CreateCouponCode.Request
            {
                Code = "SUMMER20",
                PromotionId = Guid.NewGuid()
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("SUMMER20");

        var saved = await _dbContext.Set<CouponCode>().FirstAsync(c => c.Code == "SUMMER20", TestContext.Current.CancellationToken);
        saved.State.Should().Be(CouponCodeState.Active);
    }
}
