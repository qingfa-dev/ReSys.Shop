using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Create;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionRules.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "CreatePromotionRule")]
public class CreatePromotionRuleTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreatePromotionRule.CommandHandler _handler;

    public CreatePromotionRuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreatePromotionRule.CommandHandler(_dbContext, NullLogger<CreatePromotionRule.CommandHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create rule when valid")]
    public async Task Handle_ShouldCreateRule_WhenValid()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var type = "MinimumPurchaseAmount";
        var preferences = new Dictionary<string, string> { ["Amount"] = "100" };
        var request = new CreatePromotionRule.Request
        {
            PromotionId = promotion.Id,
            Type = type,
            Preferences = preferences
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionRule.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<PromotionRule>().FirstAsync(r => r.PromotionId == promotion.Id, TestContext.Current.CancellationToken);
        saved.Type.Should().Be(type);
    }

    [Fact(DisplayName = "Handler: Should return not found when promotion does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenPromotionNotFound()
    {
        // Arrange
        var request = new CreatePromotionRule.Request
        {
            PromotionId = Guid.NewGuid(),
            Type = "MinimumPurchaseAmount",
            Preferences = []
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionRule.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }

    [Fact(DisplayName = "Handler: Should create rule with empty type")]
    public async Task Handle_ShouldCreateRule_WithEmptyType()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreatePromotionRule.Request
        {
            PromotionId = promotion.Id,
            Type = string.Empty,
            Preferences = []
        };

        // Act
        var result = await _handler.Handle(
            new CreatePromotionRule.Command(request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
