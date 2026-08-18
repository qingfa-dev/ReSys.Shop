using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.Update;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "UpdateStockItem")]
public class UpdateStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateStockItem.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public UpdateStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateStockItem.CommandHandler(
            _dbContext,
            new Mock<ILogger<UpdateStockItem.CommandHandler>>().Object,
            new Mock<ICurrentUser>().Object);
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
            CountOnHand = 5,
            Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    [Fact(DisplayName = "Handler: Updates quantity and backorderable when found")]
    public async Task Handle_UpdatesEntity_WhenFound()
    {
        var item = await SeedStockItem();

        var result = await _handler.Handle(
            new UpdateStockItem.Command(item.Id, new UpdateStockItem.Request
            {
                StockLocationId = _stockLocationId,
                VariantId = _variantId,
                CountOnHand = 25,
                Backorderable = true
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CountOnHand.Should().Be(25);
        result.Value.Backorderable.Should().BeTrue();

        var fresh = await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == item.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        fresh.CountOnHand.Should().Be(25);
        fresh.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Returns not-found when stock item does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new UpdateStockItem.Command(Guid.NewGuid(), new UpdateStockItem.Request
            {
                StockLocationId = _stockLocationId,
                VariantId = _variantId,
                CountOnHand = 1,
                Backorderable = false
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
