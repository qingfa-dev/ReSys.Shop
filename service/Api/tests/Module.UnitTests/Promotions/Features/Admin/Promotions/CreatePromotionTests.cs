using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Create;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "CreatePromotion")]
public class CreatePromotionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreatePromotion.CommandHandler _handler;

    public CreatePromotionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreatePromotion.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create promotion successfully")]
    public async Task Handle_ShouldCreatePromotion()
    {
        var result = await _handler.Handle(
            new CreatePromotion.Command(new CreatePromotion.Request
            {
                Name = "Summer Sale",
                Code = "SUMMER20",
                Description = "20% off summer collection",
                Active = true,
                Kind = PromotionKind.Automatic,
                MatchPolicy = MatchPolicy.All
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Summer Sale");

        var saved = await _dbContext.Set<Promotion>().FirstAsync(p => p.Name == "Summer Sale", TestContext.Current.CancellationToken);
        saved.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should create promotion with coupon kind")]
    public async Task Handle_ShouldCreateCouponPromotion()
    {
        var result = await _handler.Handle(
            new CreatePromotion.Command(new CreatePromotion.Request
            {
                Name = "Flash Deal",
                Kind = PromotionKind.CouponCode,
                Active = true
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<Promotion>().FirstAsync(p => p.Name == "Flash Deal", TestContext.Current.CancellationToken);
        saved.Kind.Should().Be(PromotionKind.CouponCode);
    }
}
