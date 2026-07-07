using BuildingBlocks.Querying.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Get.All;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionRules.Get.All;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetPromotionRules")]
public class GetPromotionRulesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPromotionRules.PagedQueryHandler _handler;

    public GetPromotionRulesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPromotionRules.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return rules when promotion has rules")]
    public async Task Handle_ShouldReturnRules_WhenPromotionHasRules()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);

        var rule1 = PromotionRuleExtensions.Create("RuleType1", promotionId: promotion.Id).Value;
        var rule2 = PromotionRuleExtensions.Create("RuleType2", promotionId: promotion.Id).Value;
        _dbContext.Set<PromotionRule>().AddRange(rule1, rule2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionRules.Query(promotion.Id, new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty list when promotion has no rules")]
    public async Task Handle_ShouldReturnEmptyList_WhenPromotionHasNoRules()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionRules.Query(promotion.Id, new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
