using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
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
}
