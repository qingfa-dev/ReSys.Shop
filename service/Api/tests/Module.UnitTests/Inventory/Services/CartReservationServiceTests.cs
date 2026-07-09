using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CartReservationService")]
public class CartReservationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CartReservationService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly string _cartToken = "cart-test-123";

    public CartReservationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new CartReservationService(_dbContext);
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
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    private async Task<StockReservation> SeedCartReservation(int quantity, string? cartToken = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, cartToken ?? _cartToken, DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    #region ReserveForCartAsync

    [Fact(DisplayName = "ReserveForCartAsync: Should create reservation with CartToken")]
    public async Task ReserveForCartAsync_ShouldCreateReservation_WithCartToken()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveForCartAsync(_variantId, 3, _stockLocationId, _cartToken, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.CartToken.Should().Be(_cartToken);
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.Quantity.Should().Be(3);
        result.Value.State.Should().Be(ReservationState.Reserved);
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(15), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should use custom TTL")]
    public async Task ReserveForCartAsync_ShouldUseCustomTtl()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveForCartAsync(_variantId, 1, _stockLocationId, _cartToken, ttlMinutes: 5, cancellationToken: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should return failure when quantity zero")]
    public async Task ReserveForCartAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var result = await _service.ReserveForCartAsync(_variantId, 0, _stockLocationId, _cartToken,
            cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should return failure when insufficient stock")]
    public async Task ReserveForCartAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(2);
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.ReserveForCartAsync(_variantId, 2, _stockLocationId, _cartToken, cancellationToken: ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should account for other active reservations")]
    public async Task ReserveForCartAsync_ShouldAccountForOtherActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var otherReservation = await SeedReservation(8, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));
        otherReservation.CartToken = "other-cart";
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.ReserveForCartAsync(_variantId, 3, _stockLocationId, _cartToken, cancellationToken: ct);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ReleaseCartReservationsAsync

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should release and restore stock")]
    public async Task ReleaseCartReservationsAsync_ShouldReleaseAndRestoreStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedCartReservation(2);
        await SeedCartReservation(3);

        await _service.ReleaseCartReservationsAsync(_cartToken, ct);
        await _dbContext.SaveChangesAsync(ct);

        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == _cartToken).ToListAsync(ct);
        reservations.Should().HaveCount(2);
        reservations.Should().AllSatisfy(r => r.State.Should().Be(ReservationState.Released));

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should not affect other carts")]
    public async Task ReleaseCartReservationsAsync_ShouldNotAffectOtherCarts()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedCartReservation(2, _cartToken);
        await SeedCartReservation(3, "other-cart");

        await _service.ReleaseCartReservationsAsync(_cartToken, ct);
        await _dbContext.SaveChangesAsync(ct);

        var other = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.CartToken == "other-cart", ct);
        other.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should handle empty reservations")]
    public async Task ReleaseCartReservationsAsync_ShouldHandleEmptyReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await _service.ReleaseCartReservationsAsync("nonexistent-cart", ct);
    }

    #endregion

    #region GetReservationsForCartAsync

    [Fact(DisplayName = "GetReservationsForCartAsync: Should return active reservations with remaining seconds")]
    public async Task GetReservationsForCartAsync_ShouldReturnActiveReservations_WithRemainingSeconds()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedCartReservation(2);
        await SeedCartReservation(3);

        var result = await _service.GetReservationsForCartAsync(_cartToken, ct);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.RemainingSeconds.Should().BeGreaterThan(0));
        result.Select(r => r.Reservation.Quantity).Should().BeEquivalentTo([2, 3]);
    }

    [Fact(DisplayName = "GetReservationsForCartAsync: Should return empty when no reservations")]
    public async Task GetReservationsForCartAsync_ShouldReturnEmpty_WhenNoReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.GetReservationsForCartAsync("nonexistent", ct);
        result.Should().BeEmpty();
    }

    #endregion
}
