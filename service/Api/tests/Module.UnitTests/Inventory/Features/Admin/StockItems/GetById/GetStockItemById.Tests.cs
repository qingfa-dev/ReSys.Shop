using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.GetById;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.GetById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemGetById")]
public class GetStockItemByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockItemById.QueryHandler _handler;

    public GetStockItemByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetStockItemById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return stock item when found")]
    public async Task Handle_ShouldReturnStockItem_WhenFound()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetStockItemById.Query(item.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(item.Id);
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new GetStockItemById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockItemResult.Errors.NotFound(Guid.Empty).Code);
    }
}
