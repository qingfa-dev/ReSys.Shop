using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Get.ById;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetPromotionById")]
public class GetPromotionByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPromotionById.QueryHandler _handler;

    public GetPromotionByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPromotionById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return promotion when found")]
    public async Task Handle_ShouldReturnPromotion_WhenFound()
    {
        // Arrange
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Summer Sale",
            Code = "SUMMER20",
            Active = true,
            Kind = PromotionKind.Automatic,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionById.Query(promotion.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(promotion.Id);
        result.Value.Name.Should().Be("Summer Sale");
        result.Value.Code.Should().Be("SUMMER20");
    }

    [Fact(DisplayName = "Handler: Should return not found when promotion does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new GetPromotionById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }
}
