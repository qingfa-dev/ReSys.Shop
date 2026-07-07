using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Update;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionCategories.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "UpdatePromotionCategory")]
public class UpdatePromotionCategoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdatePromotionCategory.CommandHandler _handler;

    public UpdatePromotionCategoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PromotionCategory).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdatePromotionCategory.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update promotion category name")]
    public async Task Handle_ShouldUpdateName()
    {
        // Arrange
        var category = new PromotionCategory
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Code = "OLD",
            Presentation = "Old Presentation",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<PromotionCategory>().Add(category);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdatePromotionCategory.Command(category.Id, new UpdatePromotionCategory.Request
            {
                Name = "New Name"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Code.Should().Be("OLD");
        result.Value.Presentation.Should().Be("Old Presentation");
    }

    [Fact(DisplayName = "Handler: Should update promotion category code")]
    public async Task Handle_ShouldUpdateCode()
    {
        // Arrange
        var category = new PromotionCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Code = "OLDCODE",
            Presentation = "Old Presentation",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<PromotionCategory>().Add(category);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdatePromotionCategory.Command(category.Id, new UpdatePromotionCategory.Request
            {
                Code = "NEWCODE"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Category");
        result.Value.Code.Should().Be("NEWCODE");
    }

    [Fact(DisplayName = "Handler: Should update promotion category presentation")]
    public async Task Handle_ShouldUpdatePresentation()
    {
        // Arrange
        var category = new PromotionCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Code = "TEST",
            Presentation = "Old Presentation",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<PromotionCategory>().Add(category);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdatePromotionCategory.Command(category.Id, new UpdatePromotionCategory.Request
            {
                Presentation = "New Presentation"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Category");
        result.Value.Presentation.Should().Be("New Presentation");
    }

    [Fact(DisplayName = "Handler: Should return not found when category does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new UpdatePromotionCategory.Command(Guid.NewGuid(), new UpdatePromotionCategory.Request
            {
                Name = "New Name"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "PromotionCategory.NotFound");
    }
}
