namespace Module.UnitTests.Inventory.Features.Admin.StockItems.BulkAdjust;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
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

    private async Task<StockItem> SeedStockItem(int countOnHand, Guid? variantId = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = variantId ?? _variantId, StockLocationId = _stockLocationId,
            CountOnHand = countOnHand
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);
        return stockItem;
    }

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

    [Fact(DisplayName = "Handler: Should adjust quantity on success")]
    public async Task Handle_ShouldAdjustQuantity_OnSuccess()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = await SeedStockItem(10);

        var request = new BulkAdjustStockItems.Request
        {
            Items = [new() { StockItemId = stockItem.Id, Quantity = 5 }],
            Reason = "cycle count"
        };

        var result = await _handler.Handle(new BulkAdjustStockItems.Command(request), ct);

        result.IsSuccess.Should().BeTrue();
        var fresh = await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == stockItem.Id, cancellationToken: ct);
        fresh.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Handler: Should return not-found for unknown stock item")]
    public async Task Handle_ShouldReturnNotFound_WhenStockItemMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BulkAdjustStockItems.Request
        {
            Items = [new() { StockItemId = Guid.NewGuid(), Quantity = 5 }],
            Reason = "test"
        };

        var result = await _handler.Handle(new BulkAdjustStockItems.Command(request), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "Handler: Should adjust multiple stock items in one operation")]
    public async Task Handle_ShouldAdjustMultipleItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var itemA = await SeedStockItem(10, variantId: Guid.NewGuid());
        var itemB = await SeedStockItem(20, variantId: Guid.NewGuid());

        var request = new BulkAdjustStockItems.Request
        {
            Items =
            [
                new() { StockItemId = itemA.Id, Quantity = 2 },
                new() { StockItemId = itemB.Id, Quantity = -5 }
            ],
            Reason = "bulk"
        };

        var result = await _handler.Handle(new BulkAdjustStockItems.Command(request), ct);

        result.IsSuccess.Should().BeTrue();
        (await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == itemA.Id, cancellationToken: ct)).CountOnHand.Should().Be(12);
        (await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == itemB.Id, cancellationToken: ct)).CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Handler: Should create a movement for each adjusted item")]
    public async Task Handle_ShouldCreateMovement_ForEachAdjustedItem()
    {
        var ct = TestContext.Current.CancellationToken;
        var itemA = await SeedStockItem(10, variantId: Guid.NewGuid());
        var itemB = await SeedStockItem(20, variantId: Guid.NewGuid());

        var request = new BulkAdjustStockItems.Request
        {
            Items =
            [
                new() { StockItemId = itemA.Id, Quantity = 3 },
                new() { StockItemId = itemB.Id, Quantity = -4 }
            ],
            Reason = "audit"
        };

        var result = await _handler.Handle(new BulkAdjustStockItems.Command(request), ct);

        result.IsSuccess.Should().BeTrue();
        var movements = await _dbContext.Set<StockMovement>().ToListAsync(cancellationToken: ct);
        movements.Should().HaveCount(2);
        movements.Should().Contain(m => m.StockItemId == itemA.Id && m.Quantity == 3);
        movements.Should().Contain(m => m.StockItemId == itemB.Id && m.Quantity == -4);
        movements.Should().AllSatisfy(m => m.OriginatorType.Should().Be("Adjustment"));
    }
}
