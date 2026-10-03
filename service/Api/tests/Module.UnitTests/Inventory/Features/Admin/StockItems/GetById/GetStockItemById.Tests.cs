using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.GetById;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.GetById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockItemById")]
public class GetStockItemByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockItemById.QueryHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

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

    private async Task<StockItem> SeedStockItem()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem
        {
            VariantId = _variantId,
            StockLocationId = _stockLocationId,
            CountOnHand = 7,
            Backorderable = true
        };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    [Fact(DisplayName = "Handle: Returns stock item DTO when found")]
    public async Task Handle_ReturnsDto_WhenFound()
    {
        var item = await SeedStockItem();

        var result = await _handler.Handle(new GetStockItemById.Query(item.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(item.Id);
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.StockLocationId.Should().Be(_stockLocationId);
        result.Value.CountOnHand.Should().Be(7);
        result.Value.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Returns not-found when stock item does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new GetStockItemById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
