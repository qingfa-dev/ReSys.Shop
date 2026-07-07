using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Delete;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "DeletePromotion")]
public class DeletePromotionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeletePromotion.CommandHandler _handler;

    public DeletePromotionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeletePromotion.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should soft-delete promotion")]
    public async Task Handle_ShouldSoftDelete()
    {
        // Arrange
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Test Promotion",
            Active = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DeletePromotion.Command(promotion.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Set<Promotion>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == promotion.Id, TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
        saved!.IsDeleted.Should().BeTrue();
        saved.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return not found when promotion does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new DeletePromotion.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }
}
