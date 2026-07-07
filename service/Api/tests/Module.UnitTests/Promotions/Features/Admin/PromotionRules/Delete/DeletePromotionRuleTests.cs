using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Delete;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionRules.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "DeletePromotionRule")]
public class DeletePromotionRuleTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeletePromotionRule.CommandHandler _handler;

    public DeletePromotionRuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeletePromotionRule.CommandHandler(_dbContext, NullLogger<DeletePromotionRule.CommandHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete rule when valid")]
    public async Task Handle_ShouldDeleteRule_WhenValid()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);

        var rule = PromotionRuleExtensions.Create("MinimumPurchaseAmount", promotionId: promotion.Id).Value;
        _dbContext.Set<PromotionRule>().Add(rule);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeletePromotionRule.Command(promotion.Id, rule.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exists = await _dbContext.Set<PromotionRule>().AnyAsync(r => r.Id == rule.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when rule does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenRuleNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new DeletePromotionRule.Command(Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "PromotionRule.NotFound");
    }
}
