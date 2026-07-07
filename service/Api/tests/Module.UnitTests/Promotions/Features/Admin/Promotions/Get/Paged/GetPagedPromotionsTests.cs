using BuildingBlocks.Querying.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Get.Paged;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "GetPagedPromotions")]
public class GetPagedPromotionsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedPromotions.PagedQueryHandler _handler;

    public GetPagedPromotionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPagedPromotions.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return promotions ordered by created date")]
    public async Task Handle_ShouldReturnPromotions_OrderedByCreatedAtUtcDesc()
    {
        // Arrange
        var older = new Promotion { Id = Guid.NewGuid(), Name = "Older", Active = true, CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-2) };
        var newer = new Promotion { Id = Guid.NewGuid(), Name = "Newer", Active = true, CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-1) };

        _dbContext.Set<Promotion>().AddRange(older, newer);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPagedPromotions.Query(new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Newer");
    }
}
