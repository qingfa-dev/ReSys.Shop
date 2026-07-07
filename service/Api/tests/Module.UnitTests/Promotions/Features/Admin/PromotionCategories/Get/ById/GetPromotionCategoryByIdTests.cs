using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Get.ById;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionCategories.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetPromotionCategoryById")]
public class GetPromotionCategoryByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPromotionCategoryById.QueryHandler _handler;

    public GetPromotionCategoryByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PromotionCategory).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPromotionCategoryById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return promotion category when found")]
    public async Task Handle_ShouldReturnCategory_WhenFound()
    {
        // Arrange
        var category = new PromotionCategory
        {
            Id = Guid.NewGuid(),
            Name = "Seasonal Sales",
            Code = "SEASONAL",
            Presentation = "Seasonal Promotions",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<PromotionCategory>().Add(category);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionCategoryById.Query(category.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("Seasonal Sales");
        result.Value.Code.Should().Be("SEASONAL");
        result.Value.Presentation.Should().Be("Seasonal Promotions");
    }

    [Fact(DisplayName = "Handler: Should return not found when category does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new GetPromotionCategoryById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "PromotionCategory.NotFound");
    }
}
