namespace Module.UnitTests.Inventory.Features.Admin.StockItems.BulkAdjust;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.BulkAdjust;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "BulkAdjustStockItems")]
public class BulkAdjustStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BulkAdjustStockItems.CommandHandler _handler;
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();

    public BulkAdjustStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<BulkAdjustStockItems.CommandHandler>>().Object;
        var currentUser = new Mock<ICurrentUser>().Object;
        _handler = new BulkAdjustStockItems.CommandHandler(_dbContext, logger, currentUser);
    }

    public void Dispose() { _dbContext.Dispose(); }

    [Fact(DisplayName = "Handler: Should reject negative adjustment that pushes CountOnHand below zero")]
    public async Task Handle_ShouldRejectNegativeAdjustment_WhenCountWouldGoNegative()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 3
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var request = new BulkAdjustStockItems.Request
        {
            Items = [new() { StockItemId = stockItem.Id, Quantity = -5 }],
            Reason = "test"
        };

        var result = await _handler.Handle(new BulkAdjustStockItems.Command(request), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "StockItem.CountOnHand.Negative");
    }
}
