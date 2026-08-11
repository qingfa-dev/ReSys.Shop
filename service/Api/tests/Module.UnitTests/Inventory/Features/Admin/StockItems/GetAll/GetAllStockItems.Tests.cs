using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.GetAll;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.GetAll;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetAllStockItems")]
public class GetAllStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAllStockItems.PagedQueryHandler _handler;

    public GetAllStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetAllStockItems.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Returns all items in one page when no paging params")]
    public async Task Handle_ReturnsAll_WhenNoPagingParams()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 3; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = i + 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetAllStockItems.Query(new GetAllStockItems.Parameters()), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact(DisplayName = "Handle: Pages when page/pageSize supplied")]
    public async Task Handle_Pages_WhenParamsSupplied()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 5; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = i + 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetAllStockItems.Query(new GetAllStockItems.Parameters { PageNumber = 2, PageSize = 2 }), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact(DisplayName = "Handle: Filters by Backorderable")]
    public async Task Handle_Filters_ByBackorderable()
    {
        var ct = TestContext.Current.CancellationToken;
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 5, Backorderable = false });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 6, Backorderable = false });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 7, Backorderable = true });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetAllStockItems.Query(new GetAllStockItems.Parameters { Filter = "Backorderable=false" }), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(i => i.Backorderable.Should().BeFalse());
    }

    [Fact(DisplayName = "Handle: Ignores disallowed filter field")]
    public async Task Handle_Ignores_DisallowedFilterField()
    {
        var ct = TestContext.Current.CancellationToken;
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 1 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 2 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetAllStockItems.Query(new GetAllStockItems.Parameters { Filter = "NonExistent=1" }), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handle: Sorts by CountOnHand descending")]
    public async Task Handle_Sorts_ByCountOnHandDescending()
    {
        var ct = TestContext.Current.CancellationToken;
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 1 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 2 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetAllStockItems.Query(new GetAllStockItems.Parameters { Sort = ["CountOnHand:desc"] }), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeInDescendingOrder(i => i.CountOnHand);
    }
}
