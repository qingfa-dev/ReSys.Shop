using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockReservationService")]
public class StockReservationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockReservationService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public StockReservationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockReservationService(_dbContext);
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
        int quantity, ReservationState state, DateTimeOffset? expiresAtUtc = null, Guid? orderId = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, state, expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderId ?? _orderId, createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    #region ReserveAsync

    [Fact(DisplayName = "ReserveAsync: Should create reservation with correct properties")]
    public async Task ReserveAsync_ShouldCreateReservation_WithCorrectProperties()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var result = await _service.ReserveAsync(_variantId, 3, _stockLocationId, _orderId, cancellationToken: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.Quantity.Should().Be(3);
        result.Value.State.Should().Be(ReservationState.Reserved);
        result.Value.ExpiresAtUtc.Should().BeCloseTo(
            DateTimeOffset.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));

        await _dbContext.SaveChangesAsync(ct);
        var saved = await _dbContext.Set<StockReservation>()
            .FindAsync([result.Value.Id], ct);
        saved.Should().NotBeNull();
        saved!.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when quantity is zero")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var result = await _service.ReserveAsync(_variantId, 0, _stockLocationId, _orderId,
            cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should use default TTL of 30 minutes")]
    public async Task ReserveAsync_ShouldUseDefaultTtl()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var result = await _service.ReserveAsync(_variantId, 1, _stockLocationId, _orderId, cancellationToken: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveAsync: Should prevent oversell under concurrent reservations",
        Skip = "InMemory does not support serializable isolation. Requires PostgreSQL integration test.")]
    [Trait("Category", "Integration")]
    public async Task ReserveAsync_ShouldPreventOversell_UnderConcurrentReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);

        var tasks = Enumerable.Range(0, 3)
            .Select(_ => _service.ReserveAsync(_variantId, 3, _stockLocationId, Guid.NewGuid(), cancellationToken: ct))
            .ToList();

        var results = await Task.WhenAll(tasks);

        var successes = results.Count(r => r.IsSuccess);
        successes.Should().Be(1);
    }

    #endregion

    #region ReleaseReservationsAsync

    [Fact(DisplayName = "ReleaseReservationsAsync: Should release all active reservations for order")]
    public async Task ReleaseReservationsAsync_ShouldReleaseAllActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var otherOrderId = Guid.NewGuid();
        await SeedStockItem(10);
        await SeedReservation(2, ReservationState.Reserved);
        await SeedReservation(5, ReservationState.Reserved);
        await SeedReservation(1, ReservationState.Released);
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 10, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, otherOrderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        await _service.ReleaseReservationsAsync(_orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        var ourReservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.OrderId == _orderId).ToListAsync(ct);
        ourReservations.Should().HaveCount(3);
        ourReservations.Where(r => r.State == ReservationState.Released).Should().HaveCount(3);

        var otherReservation = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.OrderId == otherOrderId, ct);
        otherReservation.State.Should().Be(ReservationState.Reserved);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(17);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should handle empty reservations list")]
    public async Task ReleaseReservationsAsync_ShouldHandleEmptyReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var emptyOrderId = Guid.NewGuid();
        await _service.ReleaseReservationsAsync(emptyOrderId, ct);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should not double-restore stock on repeated calls")]
    public async Task ReleaseReservationsAsync_ShouldNotDoubleRestoreStock_OnRepeatedCalls()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var orderId = Guid.NewGuid();
        var reservation = await SeedReservation(3, ReservationState.Reserved, orderId: orderId);

        await _service.ReleaseReservationsAsync(orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterFirst = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterFirst.CountOnHand.Should().Be(13);

        await _service.ReleaseReservationsAsync(orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterSecond = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterSecond.CountOnHand.Should().Be(13);
    }

    #endregion

    #region ExpireReservationsAsync

    [Fact(DisplayName = "ExpireReservationsAsync: Should expire past-TTL reservations, keep future")]
    public async Task ExpireReservationsAsync_ShouldExpirePastTtlOnly()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        await SeedReservation(3, ReservationState.Reserved, now.AddMinutes(-10));
        await SeedReservation(2, ReservationState.Reserved, now.AddMinutes(-5));
        await SeedReservation(4, ReservationState.Reserved, now.AddMinutes(30));

        await _service.ExpireReservationsAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var all = await _dbContext.Set<StockReservation>().ToListAsync(ct);
        var expired = all.Where(r => r.State == ReservationState.Expired).ToList();
        expired.Should().HaveCount(2);
        var active = all.Where(r => r.State == ReservationState.Reserved).ToList();
        active.Should().HaveCount(1);
        active[0].Quantity.Should().Be(4);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should handle empty expired list")]
    public async Task ExpireReservationsAsync_ShouldHandleEmptyExpired()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));
        await _service.ExpireReservationsAsync(ct);
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Quantity == 1, ct);
        fresh.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should restore stock for expired reservations")]
    public async Task ExpireReservationsAsync_ShouldRestoreStockForExpiredReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-5));

        await _service.ExpireReservationsAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(8);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should not double-restore stock on repeated calls")]
    public async Task ExpireReservationsAsync_ShouldNotDoubleRestoreStock_OnRepeatedCalls()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var reservation = await SeedReservation(3, ReservationState.Reserved, expiresAtUtc: DateTimeOffset.UtcNow.AddSeconds(-1));

        await _service.ExpireReservationsAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterFirst = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterFirst.CountOnHand.Should().Be(13); // 10 + 3

        await _service.ExpireReservationsAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterSecond = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterSecond.CountOnHand.Should().Be(13); // still 13, not 16
    }

    #endregion

    #region FulfillReservationAsync

    [Fact(DisplayName = "FulfillReservationAsync: Should transition to Fulfilled")]
    public async Task FulfillReservationAsync_ShouldTransitionToFulfilled()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.FulfillReservationAsync(reservation.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Id == reservation.Id, ct);
        fresh.State.Should().Be(ReservationState.Fulfilled);
        fresh.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "FulfillReservationAsync: Should return failure when not found")]
    public async Task FulfillReservationAsync_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _service.FulfillReservationAsync(Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "FulfillReservationAsync: Should return failure when not Reserved")]
    public async Task FulfillReservationAsync_ShouldReturnFailure_WhenNotReserved()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = await SeedReservation(3, ReservationState.Released);

        var result = await _service.FulfillReservationAsync(reservation.Id, ct);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ExpireReservationsAndRestoreStockAsync

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should expire and restore stock")]
    public async Task ExpireReservationsAndRestoreStock_ShouldExpireAndRestoreStock()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        await SeedStockItem(5);
        await SeedReservation(2, ReservationState.Reserved, now.AddMinutes(-10));
        await SeedReservation(3, ReservationState.Reserved, now.AddMinutes(-5));
        await SeedReservation(1, ReservationState.Reserved, now.AddMinutes(30));

        var count = await _service.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        count.Should().Be(2);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10);

        var future = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.Quantity == 1, ct);
        future.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should return zero when no expired")]
    public async Task ExpireReservationsAndRestoreStock_ShouldReturnZero_WhenNoExpired()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var count = await _service.ExpireReservationsAndRestoreStockAsync(ct);

        count.Should().Be(0);
    }

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should handle null StockLocationId")]
    public async Task ExpireReservationsAndRestoreStock_ShouldHandleNullStockLocationId()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, now.AddMinutes(-5),
            orderId: _orderId, createdAtUtc: now);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);

        var count = await _service.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        count.Should().Be(1);
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Id == reservation.Id, ct);
        fresh.State.Should().Be(ReservationState.Expired);
    }

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should not affect Released or Fulfilled")]
    public async Task ExpireReservationsAndRestoreStock_ShouldNotAffectReleasedOrFulfilled()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        await SeedReservation(2, ReservationState.Released, now.AddMinutes(-10));
        await SeedReservation(3, ReservationState.Fulfilled, now.AddMinutes(-10));

        var count = await _service.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        count.Should().Be(0);
    }

    [Fact(DisplayName = "ExpireReservationsAndRestoreStockAsync: Should not double-restore stock on repeated calls")]
    public async Task ExpireReservationsAndRestoreStockAsync_ShouldNotDoubleRestoreStock_OnRepeatedCalls()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, expiresAtUtc: DateTimeOffset.UtcNow.AddSeconds(-1));

        await _service.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterFirst = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterFirst.CountOnHand.Should().Be(13);

        await _service.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockAfterSecond = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockAfterSecond.CountOnHand.Should().Be(13); // still 13, not 16
    }

    #endregion
}
