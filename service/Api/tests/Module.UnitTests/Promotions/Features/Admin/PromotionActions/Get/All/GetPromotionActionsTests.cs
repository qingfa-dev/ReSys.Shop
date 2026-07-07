using BuildingBlocks.Querying.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Get.All;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionActions.Get.All;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetPromotionActions")]
public class GetPromotionActionsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPromotionActions.PagedQueryHandler _handler;

    public GetPromotionActionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPromotionActions.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return actions when promotion has actions")]
    public async Task Handle_ShouldReturnActions_WhenPromotionHasActions()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);

        var action1 = PromotionActionExtensions.Create("ActionType1", promotionId: promotion.Id).Value;
        var action2 = PromotionActionExtensions.Create("ActionType2", promotionId: promotion.Id).Value;
        _dbContext.Set<PromotionAction>().AddRange(action1, action2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionActions.Query(promotion.Id, new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty list when promotion has no actions")]
    public async Task Handle_ShouldReturnEmptyList_WhenPromotionHasNoActions()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPromotionActions.Query(promotion.Id, new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
