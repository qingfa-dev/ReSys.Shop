using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Get.Paged;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationList")]
public class GetPagedStockLocationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedStockLocations.PagedQueryHandler _handler;

    public GetPagedStockLocationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetPagedStockLocations.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return paged list of stock locations")]
    public async Task Handle_ShouldReturnPagedResults()
    {
        // Arrange: Seed data
        _dbContext.Set<StockLocation>().AddRange(
            StockLocationMethod.Create("Location A").Value,
            StockLocationMethod.Create("Location B").Value,
            StockLocationMethod.Create("Location C").Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetPagedStockLocations.Query(new QueryingParameters { PageSize = 2, PageNumber = 0 });

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Handler: Should return all locations when page size is zero")]
    public async Task Handle_ShouldReturnAll_WhenPageSizeZero()
    {
        // Arrange
        _dbContext.Set<StockLocation>().AddRange(
            StockLocationMethod.Create("Alpha").Value,
            StockLocationMethod.Create("Beta").Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetPagedStockLocations.Query(new QueryingParameters()),
            TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
    }
}
