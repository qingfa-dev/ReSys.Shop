using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.GetById;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.GetById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationGetById")]
public class GetStockLocationByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockLocationById.QueryHandler _handler;

    public GetStockLocationByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetStockLocationById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return stock location when found")]
    public async Task Handle_ShouldReturnLocation_WhenFound()
    {
        var location = StockLocationMethod.Create("Test Warehouse", city: "Chicago").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStockLocationById.Query(location.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(location.Id);
        result.Value.Name.Should().Be("Test Warehouse");
        result.Value.City.Should().Be("Chicago");
    }

    [Fact(DisplayName = "Handler: Should return failure when not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new GetStockLocationById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Errors.NotFound.Code);
    }
}
