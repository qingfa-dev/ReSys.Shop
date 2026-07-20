using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockRestockService")]
public class StockRestockServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockRestockService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public StockRestockServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockRestockService(_dbContext);
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

    [Fact(DisplayName = "RestockAsync: Should increase CountOnHand")]
    public async Task RestockAsync_ShouldIncreaseCountOnHand()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(10);

        var result = await _service.RestockAsync(item.Id, 20, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousCountOnHand.Should().Be(10);
        result.Value.NewCountOnHand.Should().Be(30);
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(20);

        var stockItem = await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == item.Id, ct);
        stockItem.CountOnHand.Should().Be(30);
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when quantity zero")]
    public async Task RestockAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(10);

        var result = await _service.RestockAsync(item.Id, 0, cancellationToken: ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when stock item not found")]
    public async Task RestockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var result = await _service.RestockAsync(Guid.NewGuid(), 10,
            cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "RestockAsync: Should fulfill backorders fully")]
    public async Task RestockAsync_ShouldFulfillBackordersFully()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 0, Backorderable = true
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderA, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10), reason: "backorder"));
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderB, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-5), reason: "backorder"));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.RestockAsync(stockItem.Id, 5, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(2);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(0);
        result.Value.NewCountOnHand.Should().Be(0);
    }

    [Fact(DisplayName = "RestockAsync: Should partially fulfill backorders")]
    public async Task RestockAsync_ShouldPartiallyFulfillBackorders()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 0, Backorderable = true
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 10, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow, reason: "backorder"));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.RestockAsync(stockItem.Id, 4, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.PartiallyFulfilled.Should().Be(1);
        result.Value.RemainingQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "RestockAsync: Should not fulfill backorders when not backorderable")]
    public async Task RestockAsync_ShouldNotFulfillBackorders_WhenNotBackorderable()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 5, Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.RestockAsync(stockItem.Id, 10, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.NewCountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "RestockAsync: Should create StockMovement with reference")]
    public async Task RestockAsync_ShouldCreateStockMovement_WithReference()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(5);

        var result = await _service.RestockAsync(item.Id, 10, "PO-001", "Summer restock", ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.OriginatorType.Should().Be("Restock");
        movement.Reason.Should().Be("Summer restock");
        movement.Action.Should().Be("restock");
        movement.Quantity.Should().Be(10);
    }
}
