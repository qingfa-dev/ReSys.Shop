using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Create;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionActions.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "CreatePromotionAction")]
public class CreatePromotionActionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreatePromotionAction.CommandHandler _handler;

    public CreatePromotionActionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreatePromotionAction.CommandHandler(_dbContext, NullLogger<CreatePromotionAction.CommandHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create action when valid")]
    public async Task Handle_ShouldCreateAction_WhenValid()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var type = "PercentageDiscount";
        var preferences = new Dictionary<string, string> { ["Percentage"] = "10" };
        var request = new CreatePromotionAction.Request
        {
            PromotionId = promotion.Id,
            Type = type,
            Preferences = preferences,
            CalculatorType = "PercentageCalculator"
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionAction.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<PromotionAction>().FirstAsync(a => a.PromotionId == promotion.Id, TestContext.Current.CancellationToken);
        saved.Type.Should().Be(type);
        saved.CalculatorType.Should().Be("PercentageCalculator");
    }

    [Fact(DisplayName = "Handler: Should return not found when promotion does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenPromotionNotFound()
    {
        // Arrange
        var request = new CreatePromotionAction.Request
        {
            PromotionId = Guid.NewGuid(),
            Type = "PercentageDiscount",
            Preferences = []
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionAction.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }

    [Fact(DisplayName = "Handler: Should create action without calculator type")]
    public async Task Handle_ShouldCreateAction_WithoutCalculatorType()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreatePromotionAction.Request
        {
            PromotionId = promotion.Id,
            Type = "FixedDiscount",
            Preferences = [],
            CalculatorType = null
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionAction.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<PromotionAction>().FirstAsync(a => a.PromotionId == promotion.Id, TestContext.Current.CancellationToken);
        saved.CalculatorType.Should().BeNull();
    }
}
