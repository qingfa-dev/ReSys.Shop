using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;

namespace Module.UnitTests.Inventory.Domain.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockChecker")]
public class StockCheckerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockChecker _checker;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly string _cartToken = "cart-test-123";

    public StockCheckerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _checker = new StockChecker(_dbContext);
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
        var reservation = new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = _orderId, Quantity = quantity, State = state,
            ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when enough available stock")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenEnoughAvailable()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _checker.IsAvailableAsync(_variantId, 5, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when insufficient available stock")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenInsufficientAfterReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _checker.IsAvailableAsync(_variantId, 8, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when stock item not found")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenStockItemNotFound()
    {
        var result = await _checker.IsAvailableAsync(Guid.NewGuid(), 5, Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore expired reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreExpiredReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = await _checker.IsAvailableAsync(_variantId, 8, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore released reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreReleasedReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(5, ReservationState.Released, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _checker.IsAvailableAsync(_variantId, 9, _stockLocationId,
            TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when quantity is zero")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenQuantityZero()
    {
        var result = await _checker.IsAvailableAsync(_variantId, 0, _stockLocationId,
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

        var result = await _checker.IsAvailableAnyLocationAsync(_variantId, 6, ct);
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

        var result = await _checker.IsAvailableAnyLocationAsync(_variantId, 6, ct);
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return true when quantity is zero")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnTrue_WhenQuantityZero()
    {
        var result = await _checker.IsAvailableAnyLocationAsync(_variantId, 0,
            TestContext.Current.CancellationToken);
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveAsync: Should create reservation with correct properties")]
    public async Task ReserveAsync_ShouldCreateReservation_WithCorrectProperties()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _checker.ReserveAsync(_variantId, 3, _stockLocationId, _orderId, cancellationToken: ct);

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
        var result = await _checker.ReserveAsync(_variantId, 0, _stockLocationId, _orderId,
            cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should release all active reservations for order")]
    public async Task ReleaseReservationsAsync_ShouldReleaseAllActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var otherOrderId = Guid.NewGuid();
        await SeedReservation(2, ReservationState.Reserved);
        await SeedReservation(5, ReservationState.Reserved);
        await SeedReservation(1, ReservationState.Released);
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = otherOrderId, Quantity = 10, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30), CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        await _checker.ReleaseReservationsAsync(_orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        var ourReservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.OrderId == _orderId).ToListAsync(ct);
        ourReservations.Should().HaveCount(3);
        ourReservations.Where(r => r.State == ReservationState.Released).Should().HaveCount(3);

        var otherReservation = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.OrderId == otherOrderId, ct);
        otherReservation.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should expire past-TTL reservations, keep future")]
    public async Task ExpireReservationsAsync_ShouldExpirePastTtlOnly()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        await SeedReservation(3, ReservationState.Reserved, now.AddMinutes(-10));
        await SeedReservation(2, ReservationState.Reserved, now.AddMinutes(-5));
        await SeedReservation(4, ReservationState.Reserved, now.AddMinutes(30));

        await _checker.ExpireReservationsAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        var all = await _dbContext.Set<StockReservation>().ToListAsync(ct);
        var expired = all.Where(r => r.State == ReservationState.Expired).ToList();
        expired.Should().HaveCount(2);
        var active = all.Where(r => r.State == ReservationState.Reserved).ToList();
        active.Should().HaveCount(1);
        active[0].Quantity.Should().Be(4);
    }

    [Fact(DisplayName = "DecrementStockAsync: Should reduce CountOnHand and create StockMovement")]
    public async Task DecrementStockAsync_ShouldDecrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _checker.DecrementStockAsync(_variantId, 3, _stockLocationId, _orderId, ct);
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
        var result = await _checker.DecrementStockAsync(Guid.NewGuid(), 3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "DecrementStockAsync: Should return failure when insufficient stock")]
    public async Task DecrementStockAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(3);

        var result = await _checker.DecrementStockAsync(_variantId, 5, _stockLocationId, _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "IncrementStockAsync: Should increase CountOnHand and create StockMovement")]
    public async Task IncrementStockAsync_ShouldIncrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);

        var result = await _checker.IncrementStockAsync(_variantId, 3, _stockLocationId, _orderId, ct);
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
        var result = await _checker.IncrementStockAsync(Guid.NewGuid(), 3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    // ========== ReserveForCartAsync Tests ==========

    [Fact(DisplayName = "ReserveForCartAsync: Should create reservation with CartToken")]
    public async Task ReserveForCartAsync_ShouldCreateReservation_WithCartToken()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _checker.ReserveForCartAsync(_variantId, 3, _stockLocationId, _cartToken, cancellationToken: ct);
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

        var result = await _checker.ReserveForCartAsync(_variantId, 1, _stockLocationId, _cartToken, ttlMinutes: 5, cancellationToken: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should return failure when quantity zero")]
    public async Task ReserveForCartAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var result = await _checker.ReserveForCartAsync(_variantId, 0, _stockLocationId, _cartToken,
            cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should return failure when insufficient stock")]
    public async Task ReserveForCartAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(2);
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _checker.ReserveForCartAsync(_variantId, 2, _stockLocationId, _cartToken, cancellationToken: ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReserveForCartAsync: Should account for other active reservations")]
    public async Task ReserveForCartAsync_ShouldAccountForOtherActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        // Another cart holds 8
        var otherReservation = await SeedReservation(8, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));
        otherReservation.CartToken = "other-cart";
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.ReserveForCartAsync(_variantId, 3, _stockLocationId, _cartToken, cancellationToken: ct);

        result.IsFailure.Should().BeTrue(); // 10 - 8 = 2 available, 3 requested
    }

    // ========== FulfillReservationAsync Tests ==========

    [Fact(DisplayName = "FulfillReservationAsync: Should transition to Fulfilled")]
    public async Task FulfillReservationAsync_ShouldTransitionToFulfilled()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = await SeedReservation(3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _checker.FulfillReservationAsync(reservation.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Id == reservation.Id, ct);
        fresh.State.Should().Be(ReservationState.Fulfilled);
        fresh.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "FulfillReservationAsync: Should return failure when not found")]
    public async Task FulfillReservationAsync_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _checker.FulfillReservationAsync(Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "FulfillReservationAsync: Should return failure when not Reserved")]
    public async Task FulfillReservationAsync_ShouldReturnFailure_WhenNotReserved()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = await SeedReservation(3, ReservationState.Released);

        var result = await _checker.FulfillReservationAsync(reservation.Id, ct);

        result.IsFailure.Should().BeTrue();
    }

    // ========== ReleaseCartReservationsAsync Tests ==========

    private async Task<StockReservation> SeedCartReservation(int quantity, string? cartToken = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = _orderId, Quantity = quantity, State = ReservationState.Reserved,
            CartToken = cartToken ?? _cartToken,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should release and restore stock")]
    public async Task ReleaseCartReservationsAsync_ShouldReleaseAndRestoreStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);
        await SeedCartReservation(2);
        await SeedCartReservation(3);

        await _checker.ReleaseCartReservationsAsync(_cartToken, ct);
        await _dbContext.SaveChangesAsync(ct);

        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == _cartToken).ToListAsync(ct);
        reservations.Should().HaveCount(2);
        reservations.Should().AllSatisfy(r => r.State.Should().Be(ReservationState.Released));

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10); // 5 + 2 + 3
    }

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should not affect other carts")]
    public async Task ReleaseCartReservationsAsync_ShouldNotAffectOtherCarts()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedCartReservation(2, _cartToken);
        await SeedCartReservation(3, "other-cart");

        await _checker.ReleaseCartReservationsAsync(_cartToken, ct);
        await _dbContext.SaveChangesAsync(ct);

        var other = await _dbContext.Set<StockReservation>()
            .FirstAsync(r => r.CartToken == "other-cart", ct);
        other.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "ReleaseCartReservationsAsync: Should handle empty reservations")]
    public async Task ReleaseCartReservationsAsync_ShouldHandleEmptyReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        await _checker.ReleaseCartReservationsAsync("nonexistent-cart", ct);
        // No exception thrown = success
    }

    // ========== ExpireReservationsAndRestoreStockAsync Tests ==========

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should expire and restore stock")]
    public async Task ExpireReservationsAndRestoreStock_ShouldExpireAndRestoreStock()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        await SeedStockItem(5);
        await SeedReservation(2, ReservationState.Reserved, now.AddMinutes(-10));
        await SeedReservation(3, ReservationState.Reserved, now.AddMinutes(-5));
        await SeedReservation(1, ReservationState.Reserved, now.AddMinutes(30));

        var count = await _checker.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        count.Should().Be(2);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(10); // 5 + 2 + 3

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

        var count = await _checker.ExpireReservationsAndRestoreStockAsync(ct);

        count.Should().Be(0);
    }

    [Fact(DisplayName = "ExpireReservationsAndRestoreStock: Should handle null StockLocationId")]
    public async Task ExpireReservationsAndRestoreStock_ShouldHandleNullStockLocationId()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        var reservation = new StockReservation
        {
            VariantId = _variantId, StockLocationId = null,
            OrderId = _orderId, Quantity = 3, State = ReservationState.Reserved,
            ExpiresAtUtc = now.AddMinutes(-5), CreatedAtUtc = now
        };
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);

        var count = await _checker.ExpireReservationsAndRestoreStockAsync(ct);
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

        var count = await _checker.ExpireReservationsAndRestoreStockAsync(ct);
        await _dbContext.SaveChangesAsync(ct);

        count.Should().Be(0);
    }

    // ========== GetReservationsForCartAsync Tests ==========

    [Fact(DisplayName = "GetReservationsForCartAsync: Should return active reservations with remaining seconds")]
    public async Task GetReservationsForCartAsync_ShouldReturnActiveReservations_WithRemainingSeconds()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedCartReservation(2);
        await SeedCartReservation(3);

        var result = await _checker.GetReservationsForCartAsync(_cartToken, ct);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.RemainingSeconds.Should().BeGreaterThan(0));
        result.Select(r => r.Reservation.Quantity).Should().BeEquivalentTo([2, 3]);
    }

    [Fact(DisplayName = "GetReservationsForCartAsync: Should return empty when no reservations")]
    public async Task GetReservationsForCartAsync_ShouldReturnEmpty_WhenNoReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _checker.GetReservationsForCartAsync("nonexistent", ct);
        result.Should().BeEmpty();
    }

    // ========== RestockAsync Tests ==========

    [Fact(DisplayName = "RestockAsync: Should increase CountOnHand")]
    public async Task RestockAsync_ShouldIncreaseCountOnHand()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(10);

        var result = await _checker.RestockAsync(item.Id, 20, cancellationToken: ct);
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

        var result = await _checker.RestockAsync(item.Id, 0, cancellationToken: ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when stock item not found")]
    public async Task RestockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var result = await _checker.RestockAsync(Guid.NewGuid(), 10,
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

        // Seed 2 backorder reservations (oldest first)
        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = orderA, Quantity = 3, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = orderB, Quantity = 2, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5)
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.RestockAsync(stockItem.Id, 5, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(2);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(0);
        result.Value.NewCountOnHand.Should().Be(0); // all 5 went to backorders
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

        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = _orderId, Quantity = 10, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.RestockAsync(stockItem.Id, 4, cancellationToken: ct);
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

        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = _orderId, Quantity = 3, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.RestockAsync(stockItem.Id, 10, cancellationToken: ct);
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

        var result = await _checker.RestockAsync(item.Id, 10, "PO-001", "Summer restock", ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.OriginatorType.Should().Be("Restock");
        movement.Reason.Should().Be("Summer restock");
        movement.Action.Should().Be("restock");
        movement.Quantity.Should().Be(10);
    }

    // ========== GetStockSummaryAsync Tests ==========

    [Fact(DisplayName = "GetStockSummaryAsync: Should return consolidated per-variant totals")]
    public async Task GetStockSummaryAsync_ShouldReturnConsolidatedPerVariant()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();

        // Seed locations
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = locA, Name = "LocA", Active = true });
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = locB, Name = "LocB", Active = true });
        await _dbContext.SaveChangesAsync(ct);

        // Seed stock items
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 5 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 7 });
        await _dbContext.SaveChangesAsync(ct);

        // Seed reservation for locA
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = locA, OrderId = _orderId,
            Quantity = 2, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30), CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.GetStockSummaryAsync(ct);

        result.Should().HaveCount(1);
        var summary = result[0];
        summary.VariantId.Should().Be(_variantId);
        summary.TotalOnHand.Should().Be(12);
        summary.TotalReserved.Should().Be(2);
        summary.TotalAvailable.Should().Be(10);
        summary.LocationBreakdown.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should flag low stock items")]
    public async Task GetStockSummaryAsync_ShouldFlagLowStockItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = loc, CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _checker.GetStockSummaryAsync(ct);

        result.Should().HaveCount(1);
        result[0].LocationBreakdown[0].IsLowStock.Should().BeTrue();
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should return empty when no stock items")]
    public async Task GetStockSummaryAsync_ShouldReturnEmpty_WhenNoStockItems()
    {
        var result = await _checker.GetStockSummaryAsync(TestContext.Current.CancellationToken);
        result.Should().BeEmpty();
    }

    // ========== Branch Gap-Fills ==========

    [Fact(DisplayName = "IsAvailableAnyLocationAsync: Should return false when total insufficient and no backorder check")]
    public async Task IsAvailableAnyLocationAsync_ShouldReturnFalse_WhenTotalInsufficient()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 2, Backorderable = false });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 2, Backorderable = true });
        await _dbContext.SaveChangesAsync(ct);

        // IsAvailableAsync only checks available >= quantity; doesn't consider backorderable
        var result = await _checker.IsAvailableAnyLocationAsync(_variantId, 6, ct);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "ReserveAsync: Should use default TTL of 30 minutes")]
    public async Task ReserveAsync_ShouldUseDefaultTtl()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _checker.ReserveAsync(_variantId, 1, _stockLocationId, _orderId, cancellationToken: ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "ReleaseReservationsAsync: Should handle empty reservations list")]
    public async Task ReleaseReservationsAsync_ShouldHandleEmptyReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        var emptyOrderId = Guid.NewGuid();
        await _checker.ReleaseReservationsAsync(emptyOrderId, ct);
        // No exception thrown = success
    }

    [Fact(DisplayName = "ExpireReservationsAsync: Should handle empty expired list")]
    public async Task ExpireReservationsAsync_ShouldHandleEmptyExpired()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));
        await _checker.ExpireReservationsAsync(ct);
        // No exception, future reservation unchanged
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Quantity == 1, ct);
        fresh.State.Should().Be(ReservationState.Reserved);
    }
}
