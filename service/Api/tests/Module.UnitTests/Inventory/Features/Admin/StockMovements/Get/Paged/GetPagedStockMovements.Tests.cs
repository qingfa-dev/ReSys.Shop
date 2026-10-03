using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Get.Paged;

namespace Module.UnitTests.Inventory.Features.Admin.StockMovements.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetPagedStockMovements")]
public class GetPagedStockMovementsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedStockMovements.PagedQueryHandler _handler;

    public GetPagedStockMovementsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPagedStockMovements.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(Guid variantId)
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem { VariantId = variantId, StockLocationId = Guid.NewGuid(), CountOnHand = 10 };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    private async Task<StockMovement> SeedMovement(Guid stockItemId, int quantity)
    {
        var ct = TestContext.Current.CancellationToken;
        var movement = new StockMovement { StockItemId = stockItemId, Quantity = quantity, PreviousCountOnHand = 0 };
        _dbContext.Set<StockMovement>().Add(movement);
        await _dbContext.SaveChangesAsync(ct);
        return movement;
    }

    [Fact(DisplayName = "Handle: Returns paged stock movements")]
    public async Task Handle_ReturnsPagedMovements()
    {
        var item = await SeedStockItem(Guid.NewGuid());
        for (var i = 0; i < 4; i++)
        {
            await SeedMovement(item.Id, i + 1);
        }

        var result = await _handler.Handle(
            new GetPagedStockMovements.Query(new GetPagedStockMovements.Parameters { PageSize = 2 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
    }

    [Fact(DisplayName = "Handle: Filters movements by stock item variant")]
    public async Task Handle_FiltersByVariant()
    {
        var variantA = Guid.NewGuid();
        var variantB = Guid.NewGuid();
        var itemA = await SeedStockItem(variantA);
        var itemB = await SeedStockItem(variantB);
        var moveA = await SeedMovement(itemA.Id, 1);
        await SeedMovement(itemB.Id, 2);

        var result = await _handler.Handle(
            new GetPagedStockMovements.Query(new GetPagedStockMovements.Parameters { VariantId = variantA }),
            TestContext.Current.CancellationToken);

        result.Items.Should().ContainSingle();
        result.Items.Single().StockItemId.Should().Be(itemA.Id);
    }
}
