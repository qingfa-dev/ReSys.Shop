using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Delete;

namespace Module.UnitTests.Promotions.Features.Admin.PromotionActions.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "DeletePromotionAction")]
public class DeletePromotionActionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeletePromotionAction.CommandHandler _handler;

    public DeletePromotionActionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeletePromotionAction.CommandHandler(_dbContext, NullLogger<DeletePromotionAction.CommandHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete action when valid")]
    public async Task Handle_ShouldDeleteAction_WhenValid()
    {
        // Arrange
        var promotion = PromotionExtensions.Create("Test Promotion").Value;
        _dbContext.Set<Promotion>().Add(promotion);

        var action = PromotionActionExtensions.Create("PercentageDiscount", promotionId: promotion.Id).Value;
        _dbContext.Set<PromotionAction>().Add(action);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeletePromotionAction.Command(promotion.Id, action.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exists = await _dbContext.Set<PromotionAction>().AnyAsync(a => a.Id == action.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when action does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenActionNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new DeletePromotionAction.Command(Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "PromotionAction.NotFound");
    }
}
