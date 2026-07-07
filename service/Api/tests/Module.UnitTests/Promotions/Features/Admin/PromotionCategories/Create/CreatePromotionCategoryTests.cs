using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Create;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionCategories.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "CreatePromotionCategory")]
public class CreatePromotionCategoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreatePromotionCategory.CommandHandler _handler;

    public CreatePromotionCategoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PromotionCategory).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreatePromotionCategory.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create category successfully")]
    public async Task Handle_ShouldCreateCategory()
    {
        // Act
        var result = await _handler.Handle(
            new CreatePromotionCategory.Command(new CreatePromotionCategory.Request
            {
                Name = "Seasonal",
                Code = "SEASONAL",
                Presentation = "Seasonal Promotions"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Seasonal");

        var saved = await _dbContext.Set<PromotionCategory>().FirstAsync(c => c.Name == "Seasonal", TestContext.Current.CancellationToken);
        saved.Code.Should().Be("SEASONAL");
    }
}
