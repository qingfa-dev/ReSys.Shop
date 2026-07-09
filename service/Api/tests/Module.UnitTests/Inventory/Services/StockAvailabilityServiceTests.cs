using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockAvailabilityService")]
public class StockAvailabilityServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockAvailabilityService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public StockAvailabilityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockAvailabilityService(_dbContext);
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

    private async Task<StockReservation> SeedReservation(
        int quantity, ReservationState state, DateTimeOffset? expiresAtUtc = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, state, expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderId: Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when enough available stock")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenEnoughAvailable()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 5, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when insufficient available stock")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenInsufficientAfterReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 8, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when stock item not found")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenStockItemNotFound()
    {
        var result = await _service.IsAvailableAsync(Guid.NewGuid(), 5, Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore expired reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreExpiredReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = await _service.IsAvailableAsync(_variantId, 8, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore released reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreReleasedReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(5, ReservationState.Released, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 9, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when quantity is zero")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenQuantityZero()
    {
        var result = await _service.IsAvailableAsync(_variantId, 0, _stockLocationId,
            TestContext.Current.CancellationToken);
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return true when any location has enough")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnTrue_WhenOneLocationHasEnough()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid(); var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 5 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 7 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.IsAvailableAnyLocationAsync(_variantId, 6, ct);
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return false when no location has enough")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnFalse_WhenNoLocationHasEnough()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid(); var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 2 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.IsAvailableAnyLocationAsync(_variantId, 6, ct);
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return true when quantity is zero")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnTrue_WhenQuantityZero()
    {
        var result = await _service.IsAvailableAnyLocationAsync(_variantId, 0,
            TestContext.Current.CancellationToken);
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return false when total insufficient")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnFalse_WhenTotalInsufficient()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 2, Backorderable = false });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 2, Backorderable = true });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.IsAvailableAnyLocationAsync(_variantId, 6, ct);

        result.Should().BeFalse();
    }
}
