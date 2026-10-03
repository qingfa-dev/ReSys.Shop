using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.Delete;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "DeleteStockItem")]
public class DeleteStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteStockItem.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public DeleteStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteStockItem.CommandHandler(_dbContext);
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
            CountOnHand = 9,
            Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    [Fact(DisplayName = "Handler: Removes stock item when found")]
    public async Task Handle_RemovesEntity_WhenFound()
    {
        var item = await SeedStockItem();

        var result = await _handler.Handle(new DeleteStockItem.Command(item.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(item.Id);
        var exists = await _dbContext.Set<StockItem>().AnyAsync(si => si.Id == item.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Returns not-found when stock item does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new DeleteStockItem.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
