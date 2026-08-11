using Microsoft.Extensions.Logging.Abstractions;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

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
    private readonly string _cartToken = "cart-test-123";

    public StockReservationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockReservationService(_dbContext, NullLogger<StockReservationService>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(int countOnHand)
    {
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = countOnHand, Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return stockItem;
    }

    private async Task<StockReservation> SeedOrderReservation(int quantity)
    {
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return reservation;
    }

    private async Task<StockReservation> SeedCartReservation(int quantity, string? cartToken = null)
    {
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, cartToken ?? _cartToken, DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return reservation;
    }

    #region ReserveAsync — Cart

    [Fact(DisplayName = "ReserveAsync: Should create reservation with CartToken")]
    public async Task ReserveAsync_ShouldCreateReservation_WithCartToken()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveAsync(_variantId, 3, _stockLocationId, cartToken: _cartToken, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.CartToken.Should().Be(_cartToken);
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.Quantity.Should().Be(3);
        result.Value.State.Should().Be(ReservationState.Reserved);
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveAsync: Should use custom TTL")]
    public async Task ReserveAsync_ShouldUseCustomTtl()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveAsync(_variantId, 1, _stockLocationId, cartToken: _cartToken, ttlMinutes: 5, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when quantity zero")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var result = await _service.ReserveAsync(_variantId, 0, _stockLocationId, cartToken: _cartToken,
            ct: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when quantity negative")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenQuantityNegative()
    {
        var result = await _service.ReserveAsync(_variantId, -1, _stockLocationId, cartToken: _cartToken,
            ct: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when neither orderId nor cartToken provided")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenNeitherOrderIdNorCartToken()
    {
        await SeedStockItem(10);

        var result = await _service.ReserveAsync(_variantId, 3, _stockLocationId, ct: TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when stock item not found")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _service.ReserveAsync(Guid.NewGuid(), 3, Guid.NewGuid(), orderId: _orderId, ct: ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should return failure when insufficient stock")]
    public async Task ReserveAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(2);
        await SeedOrderReservation(1);

        var result = await _service.ReserveAsync(_variantId, 2, _stockLocationId, cartToken: _cartToken, ct: ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should ignore expired existing reservations when computing availability")]
    public async Task ReserveAsync_ShouldIgnoreExpiredReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 8, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-10),
            _stockLocationId, Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-40)));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.ReserveAsync(_variantId, 5, _stockLocationId, cartToken: _cartToken, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(5);
    }

    #endregion

    #region ReserveAsync — Order

    [Fact(DisplayName = "ReserveAsync: Should create reservation for order")]
    public async Task ReserveAsync_ShouldCreateReservation_ForOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveAsync(_variantId, 3, _stockLocationId, orderId: _orderId, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(_orderId);
        result.Value.Quantity.Should().Be(3);
    }

    [Fact(DisplayName = "ReserveAsync: Should create reservation with both orderId and cartToken")]
    public async Task ReserveAsync_ShouldCreateReservation_WithBothOrderAndCart()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReserveAsync(_variantId, 3, _stockLocationId, orderId: _orderId, cartToken: _cartToken, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(_orderId);
        result.Value.CartToken.Should().Be(_cartToken);
    }

    #endregion

    #region ReleaseReservationsAsync

    [Fact(DisplayName = "ReleaseReservationsAsync: Should release cart reservations and restore stock")]
    public async Task ReleaseReservationsAsync_ShouldReleaseAndRestoreStock_ByCartToken()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedCartReservation(2);
        await SeedCartReservation(3);

        var result = await _service.ReleaseReservationsAsync(cartToken: _cartToken, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);

        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == _cartToken).ToListAsync(ct);
        reservations.Should().AllSatisfy(r => r.State.Should().Be(ReservationState.Released));

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should not affect other carts")]
    public async Task ReleaseReservationsAsync_ShouldNotAffectOtherCarts()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedCartReservation(2, _cartToken);
        await SeedCartReservation(3, "other-cart");

        await _service.ReleaseReservationsAsync(cartToken: _cartToken, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        var other = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.CartToken == "other-cart", ct);
        other.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should release order reservations")]
    public async Task ReleaseReservationsAsync_ShouldReleaseByOrderId()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedOrderReservation(2);
        await SeedOrderReservation(3);

        var result = await _service.ReleaseReservationsAsync(orderId: _orderId, ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should return zero when neither orderId nor cartToken provided")]
    public async Task ReleaseReservationsAsync_ShouldReturnZero_WhenNoCriteriaProvided()
    {
        var result = await _service.ReleaseReservationsAsync(ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should return zero when no reservations match")]
    public async Task ReleaseReservationsAsync_ShouldReturnZero_WhenNoMatches()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.ReleaseReservationsAsync(cartToken: "nonexistent", ct: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
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

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.RemainingSeconds.Should().BeGreaterThan(0));
        result.Value.Select(r => r.Reservation.Quantity).Should().BeEquivalentTo([2, 3]);
    }

    [Fact(DisplayName = "GetReservationsForCartAsync: Should return empty when no reservations")]
    public async Task GetReservationsForCartAsync_ShouldReturnEmpty_WhenNoReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.GetReservationsForCartAsync("nonexistent", ct);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetReservationsForCartAsync: Should not throw when ExpiresAtUtc is null")]
    public async Task GetReservationsForCartAsync_ShouldNotThrow_WhenExpiresAtUtcIsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, null,
            _stockLocationId, _orderId, _cartToken, DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.GetReservationsForCartAsync(_cartToken, ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region ExpireReservationsAsync

    [Fact(DisplayName = "ExpireReservationsAsync: Should expire overdue reservations and restore stock")]
    public async Task ExpireReservationsAsync_ShouldExpireAndRestoreStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-5),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-30)));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.ExpireReservationsAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(7);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should return zero when no expired reservations")]
    public async Task ExpireReservationsAsync_ShouldReturnZero_WhenNoExpired()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedOrderReservation(2);

        var result = await _service.ExpireReservationsAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should handle multiple expired reservations")]
    public async Task ExpireReservationsAsync_ShouldHandleMultipleExpired()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-5),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-30)));
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-10),
            _stockLocationId, Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-40)));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.ExpireReservationsAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10 + 2 + 3);
    }

    #endregion
}
