using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.LowStock;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.LowStock;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetLowStockItems")]
public class GetLowStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetLowStockItems.PagedQueryHandler _handler;

    public GetLowStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetLowStockItems.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Returns only items at or below threshold")]
    public async Task Handle_ReturnsLowStockItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 3 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 10 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetLowStockItems.Query(new GetLowStockItems.Request(), new GetLowStockItems.Parameters()), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items.First().Status.Should().Be(LowStockStatus.Low);
    }

    [Fact(DisplayName = "Handle: Marks zero-on-hand items as OutOfStock")]
    public async Task Handle_MarksZeroOnHandAsOutOfStock()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 0 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetLowStockItems.Query(new GetLowStockItems.Request(), new GetLowStockItems.Parameters()), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items.First().Status.Should().Be(LowStockStatus.OutOfStock);
    }

    [Fact(DisplayName = "Handle: Pages results when params supplied")]
    public async Task Handle_Pages_WhenParamsSupplied()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);
        for (var i = 0; i < 4; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetLowStockItems.Query(new GetLowStockItems.Request(), new GetLowStockItems.Parameters { PageSize = 2 }), ct);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
    }
}
