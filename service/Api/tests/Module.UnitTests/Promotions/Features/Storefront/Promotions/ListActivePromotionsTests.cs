using BuildingBlocks.Querying.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Storefront.Promotions;

namespace Module.UnitTests.Promotions.Features.Storefront.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "ListActivePromotions")]
public class ListActivePromotionsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListActivePromotions.PagedQueryHandler _handler;

    public ListActivePromotionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ListActivePromotions.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return only active, non-deleted, non-expired promotions")]
    public async Task Handle_ShouldReturnActivePromotions()
    {
        // Arrange
        var active = new Promotion { Id = Guid.NewGuid(), Name = "Active", Active = true, Position = 0, CreatedAtUtc = DateTimeOffset.UtcNow };
        var inactive = new Promotion { Id = Guid.NewGuid(), Name = "Inactive", Active = false, Position = 1, CreatedAtUtc = DateTimeOffset.UtcNow };
        var deleted = new Promotion { Id = Guid.NewGuid(), Name = "Deleted", Active = true, IsDeleted = true, Position = 2, CreatedAtUtc = DateTimeOffset.UtcNow };
        var expired = new Promotion { Id = Guid.NewGuid(), Name = "Expired", Active = true, ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(-1), Position = 3, CreatedAtUtc = DateTimeOffset.UtcNow };

        _dbContext.Set<Promotion>().AddRange(active, inactive, deleted, expired);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new ListActivePromotions.Query(new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Active");
    }
}
