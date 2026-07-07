using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Update;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "UpdatePromotion")]
public class UpdatePromotionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdatePromotion.CommandHandler _handler;

    public UpdatePromotionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdatePromotion.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update promotion name")]
    public async Task Handle_ShouldUpdateName()
    {
        // Arrange
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Code = "ORIGINAL",
            Kind = PromotionKind.Automatic,
            Active = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdatePromotion.Command(promotion.Id, new UpdatePromotion.Request
            {
                Name = "Updated Name"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
    }

    [Fact(DisplayName = "Handler: Should return not found when promotion does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new UpdatePromotion.Command(Guid.NewGuid(), new UpdatePromotion.Request
            {
                Name = "New Name"
            }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }
}
