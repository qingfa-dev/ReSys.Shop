using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockQuantityService")]
public class StockQuantityServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockQuantityService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public StockQuantityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockQuantityService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(int countOnHand)
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = countOnHand, Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);
        return stockItem;
    }

    [Fact(DisplayName = "DecrementStockAsync: Should reduce CountOnHand and create StockMovement")]
    public async Task DecrementStockAsync_ShouldDecrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.DecrementStockAsync(_variantId, 3, _stockLocationId, _orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(7);

        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(-3);
        movement.OriginatorType.Should().Be("Order");
        movement.Reason.Should().Be("sold");
        movement.PreviousCountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "DecrementStockAsync: Should return failure when stock item not found")]
    public async Task DecrementStockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.DecrementStockAsync(Guid.NewGuid(), 3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "DecrementStockAsync: Should return failure when insufficient stock")]
    public async Task DecrementStockAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(3);

        var result = await _service.DecrementStockAsync(_variantId, 5, _stockLocationId, _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "IncrementStockAsync: Should increase CountOnHand and create StockMovement")]
    public async Task IncrementStockAsync_ShouldIncrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);

        var result = await _service.IncrementStockAsync(_variantId, 3, _stockLocationId, _orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(8);

        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(3);
        movement.OriginatorType.Should().Be("Order");
        movement.Reason.Should().Be("returned");
        movement.PreviousCountOnHand.Should().Be(5);
    }

    [Fact(DisplayName = "IncrementStockAsync: Should return failure when stock item not found")]
    public async Task IncrementStockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.IncrementStockAsync(Guid.NewGuid(), 3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }
}
