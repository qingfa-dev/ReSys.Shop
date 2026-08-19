using Microsoft.Extensions.Logging.Abstractions;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemService")]
public class StockItemServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockItemService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public StockItemServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockItemService(_dbContext, NullLogger<StockItemService>.Instance);
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

    private async Task<StockReservation> SeedReservation(
        int quantity, ReservationState state, Guid? orderId = null, DateTimeOffset? expiresAtUtc = null)
    {
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, state, expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderId ?? _orderId, createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return reservation;
    }

    #region AdjustStockAsync — delta == 0

    [Fact(DisplayName = "AdjustStockAsync: Should return failure when delta is zero")]
    public async Task AdjustStockAsync_ShouldReturnFailure_WhenDeltaIsZero()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.AdjustStockAsync(_variantId, 0, _stockLocationId, _orderId, ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "StockItem.CountOnHand.Negative");
    }

    #endregion

    #region AdjustStockAsync — Decrement

    [Fact(DisplayName = "AdjustStockAsync: Should reduce CountOnHand and create StockMovement on negative delta")]
    public async Task AdjustStockAsync_NegativeDelta_ShouldDecrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.AdjustStockAsync(_variantId, -3, _stockLocationId, _orderId, ct);
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
    }

    [Fact(DisplayName = "AdjustStockAsync: Should return failure when stock item not found")]
    public async Task AdjustStockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.AdjustStockAsync(Guid.NewGuid(), -3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "AdjustStockAsync: Should return failure when insufficient stock")]
    public async Task AdjustStockAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(3);

        var result = await _service.AdjustStockAsync(_variantId, -5, _stockLocationId, _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "AdjustStockAsync: Should fail when available stock minus reserved is insufficient")]
    public async Task AdjustStockAsync_ShouldFail_WhenReservedStockMakesAvailableInsufficient()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, orderId: Guid.NewGuid());

        var result = await _service.AdjustStockAsync(_variantId, -8, _stockLocationId, Guid.NewGuid(), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "StockItem.InsufficientStock");
    }

    [Fact(DisplayName = "AdjustStockAsync: Should succeed when available exactly equals decrement quantity")]
    public async Task AdjustStockAsync_ShouldSucceed_WhenAvailableExactlyEqualsDecrement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.AdjustStockAsync(_variantId, -10, _stockLocationId, _orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _stockLocationId, ct);
        stockItem.CountOnHand.Should().Be(0);
    }

    [Fact(DisplayName = "AdjustStockAsync: Should fulfill matching reservation on decrement")]
    public async Task AdjustStockAsync_ShouldFulfillMatchingReservation_OnDecrement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);
        var reservation = await SeedReservation(3, ReservationState.Reserved, orderId: _orderId);

        var result = await _service.AdjustStockAsync(_variantId, -5, _stockLocationId, _orderId, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var persisted = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Id == reservation.Id, ct);
        persisted.State.Should().Be(ReservationState.Fulfilled);
    }

    [Fact(DisplayName = "AdjustStockAsync: Should fail when stock exists at different location than requested")]
    public async Task AdjustStockAsync_ShouldFail_WhenStockAtDifferentLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(10);

        var result = await _service.AdjustStockAsync(_variantId, -3, Guid.NewGuid(), _orderId, ct);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region AdjustStockAsync — Increment

    [Fact(DisplayName = "AdjustStockAsync: Should increase CountOnHand and create StockMovement on positive delta")]
    public async Task AdjustStockAsync_PositiveDelta_ShouldIncrementAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockItem(5);

        var result = await _service.AdjustStockAsync(_variantId, 3, _stockLocationId, _orderId, ct);
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
    }

    [Fact(DisplayName = "AdjustStockAsync: Should return failure for positive delta when stock item not found")]
    public async Task AdjustStockAsync_PositiveDelta_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _service.AdjustStockAsync(Guid.NewGuid(), 3, Guid.NewGuid(), _orderId, ct);
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region RestockAsync

    [Fact(DisplayName = "RestockAsync: Should increase CountOnHand")]
    public async Task RestockAsync_ShouldIncreaseCountOnHand()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(10);

        var result = await _service.RestockAsync(item.Id, 20, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousCountOnHand.Should().Be(10);
        result.Value.NewCountOnHand.Should().Be(30);
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(20);
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when stock item not found")]
    public async Task RestockAsync_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var result = await _service.RestockAsync(Guid.NewGuid(), 10, ct: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when quantity zero")]
    public async Task RestockAsync_ShouldReturnFailure_WhenQuantityZero()
    {
        var item = await SeedStockItem(10);
        var result = await _service.RestockAsync(item.Id, 0, ct: TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "RestockAsync: Should return failure when quantity negative")]
    public async Task RestockAsync_ShouldReturnFailure_WhenQuantityNegative()
    {
        var item = await SeedStockItem(10);
        var result = await _service.RestockAsync(item.Id, -5, ct: TestContext.Current.CancellationToken);
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

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10), reason: "backorder"));
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-5), reason: "backorder"));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.RestockAsync(stockItem.Id, 5, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(2);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "RestockAsync: Should partially fulfill backorders when restock quantity insufficient")]
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

        var result = await _service.RestockAsync(stockItem.Id, 4, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.PartiallyFulfilled.Should().Be(1);
        result.Value.NewCountOnHand.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "RestockAsync: Should add surplus to on-hand after all backorders fulfilled")]
    public async Task RestockAsync_ShouldAddSurplusToOnHand_AfterBackordersFulfilled()
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
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow, reason: "backorder"));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.RestockAsync(stockItem.Id, 10, ct: ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(1);
        result.Value.NewCountOnHand.Should().Be(7);
        result.Value.RemainingQuantity.Should().Be(7);
    }

    [Fact(DisplayName = "RestockAsync: Should create StockMovement with reference")]
    public async Task RestockAsync_ShouldCreateStockMovement_WithReference()
    {
        var item = await SeedStockItem(5);

        var result = await _service.RestockAsync(item.Id, 10, "PO-001", "Summer restock", TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);
        movement.Should().NotBeNull();
        movement!.OriginatorType.Should().Be("Restock");
        movement.Action.Should().Be("restock");
        movement.Quantity.Should().Be(10);
    }

    #endregion

    #region IsAvailableAsync

    [Fact(DisplayName = "IsAvailableAsync: Should return true when enough available stock at location")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenEnoughAvailable()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 5, _stockLocationId, ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when insufficient available stock")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenInsufficientAfterReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 8, _stockLocationId, ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when stock item not found at location")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenStockItemNotFound()
    {
        var result = await _service.IsAvailableAsync(Guid.NewGuid(), 5, _stockLocationId, ct: TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore expired reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreExpiredReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(3, ReservationState.Reserved, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = await _service.IsAvailableAsync(_variantId, 8, _stockLocationId, ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should ignore released reservations")]
    public async Task IsAvailableAsync_ShouldIgnoreReleasedReservations()
    {
        await SeedStockItem(10);
        await SeedReservation(5, ReservationState.Released, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _service.IsAvailableAsync(_variantId, 9, _stockLocationId, ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when quantity is zero")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenQuantityZero()
    {
        var result = await _service.IsAvailableAsync(_variantId, 0, ct: TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return true when quantity is negative")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenQuantityNegative()
    {
        var result = await _service.IsAvailableAsync(_variantId, -1, ct: TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should check any location when no stockLocationId provided")]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenAnyLocationHasEnough()
    {
        var ct = TestContext.Current.CancellationToken;
        var variant = Guid.NewGuid();
        var locA = Guid.NewGuid(); var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = variant, StockLocationId = locA, CountOnHand = 5 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = variant, StockLocationId = locB, CountOnHand = 7 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.IsAvailableAsync(variant, 6, ct: ct);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailableAsync: Should return false when no location has enough across all")]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenNoLocationHasEnough()
    {
        var ct = TestContext.Current.CancellationToken;
        var variant = Guid.NewGuid();
        var locA = Guid.NewGuid(); var locB = Guid.NewGuid();
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = variant, StockLocationId = locA, CountOnHand = 2 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = variant, StockLocationId = locB, CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.IsAvailableAsync(variant, 6, ct: ct);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region GetSnapshotForVariantAsync

    [Fact(DisplayName = "GetSnapshotForVariantAsync: no stock returns zeros")]
    public async Task GetSnapshotForVariantAsync_NoStock_ReturnsZero()
    {
        var variantId = Guid.NewGuid();
        var result = await _service.GetSnapshotForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalOnHand.Should().Be(0);
        result.Value.TotalReserved.Should().Be(0);
        result.Value.TotalAvailable.Should().Be(0);
        result.Value.Locations.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetSnapshotForVariantAsync: excludes expired reservations")]
    public async Task GetSnapshotForVariantAsync_ExcludesExpiredReservations()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SeedLocation(locationId, "WH-1", active: true);
        SeedStockItemForVariant(variantId, locationId, onHand: 10);
        SeedReservationForVariant(variantId, locationId, quantity: 5, expiresInMinutes: -10);

        var result = await _service.GetSnapshotForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Value.TotalReserved.Should().Be(0);
        result.Value.TotalAvailable.Should().Be(10);
    }

    [Fact(DisplayName = "GetSnapshotForVariantAsync: should set Backorderable true when any location backorders")]
    public async Task GetSnapshotForVariantAsync_ShouldSetBackorderableTrue()
    {
        var variantId = Guid.NewGuid();
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        SeedLocation(locA, "WH-A", active: true);
        SeedLocation(locB, "WH-B", active: true);
        SeedStockItemForVariant(variantId, locA, onHand: 0, backorderable: false);
        SeedStockItemForVariant(variantId, locB, onHand: 0, backorderable: true);

        var result = await _service.GetSnapshotForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Value.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "GetSnapshotForVariantAsync: should exclude deleted or inactive locations")]
    public async Task GetSnapshotForVariantAsync_ShouldExcludeDeletedOrInactiveLocations()
    {
        var variantId = Guid.NewGuid();
        var locActive = Guid.NewGuid();
        var locInactive = Guid.NewGuid();
        SeedLocation(locActive, "Active", active: true);
        SeedLocation(locInactive, "Inactive", active: false);
        SeedStockItemForVariant(variantId, locActive, onHand: 5);
        SeedStockItemForVariant(variantId, locInactive, onHand: 20);

        var result = await _service.GetSnapshotForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Value.Locations.Should().HaveCount(1);
        result.Value.Locations[0].StockLocationName.Should().Be("Active");
        result.Value.TotalOnHand.Should().Be(5);
    }

    [Fact(DisplayName = "GetSnapshotForVariantAsync: should compute per-location availability with reservations")]
    public async Task GetSnapshotForVariantAsync_ShouldComputePerLocationAvailability()
    {
        var variantId = Guid.NewGuid();
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        SeedLocation(locA, "LocA", active: true);
        SeedLocation(locB, "LocB", active: true);
        SeedStockItemForVariant(variantId, locA, onHand: 10);
        SeedStockItemForVariant(variantId, locB, onHand: 5);
        SeedReservationForVariant(variantId, locA, quantity: 3, expiresInMinutes: 30);

        var result = await _service.GetSnapshotForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Value.TotalOnHand.Should().Be(15);
        result.Value.TotalReserved.Should().Be(3);
        result.Value.TotalAvailable.Should().Be(12);
        var locASnapshot = result.Value.Locations.Single(l => l.StockLocationName == "LocA");
        locASnapshot.ReservedCount.Should().Be(3);
        locASnapshot.AvailableCount.Should().Be(7);
        var locBSnapshot = result.Value.Locations.Single(l => l.StockLocationName == "LocB");
        locBSnapshot.ReservedCount.Should().Be(0);
        locBSnapshot.AvailableCount.Should().Be(5);
    }

    #endregion

    #region GetStockSummaryAsync

    [Fact(DisplayName = "GetStockSummaryAsync: Should return consolidated per-variant totals")]
    public async Task GetStockSummaryAsync_ShouldReturnConsolidatedPerVariant()
    {
        var ct = TestContext.Current.CancellationToken;
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = locA, Name = "LocA", Active = true });
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = locB, Name = "LocB", Active = true });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locA, CountOnHand = 5 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = locB, CountOnHand = 7 });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            locA, _orderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.GetStockSummaryAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var summary = result.Value[0];
        summary.VariantId.Should().Be(_variantId);
        summary.TotalOnHand.Should().Be(12);
        summary.TotalReserved.Should().Be(2);
        summary.TotalAvailable.Should().Be(10);
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

        var result = await _service.GetStockSummaryAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].LocationBreakdown[0].IsLowStock.Should().BeTrue();
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should clamp negative available to zero")]
    public async Task GetStockSummaryAsync_ShouldClampNegativeAvailableToZero()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = loc, CountOnHand = 0 });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            loc, _orderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.GetStockSummaryAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].LocationBreakdown[0].Available.Should().Be(0);
        result.Value[0].TotalAvailable.Should().Be(0);
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should return empty when no stock items")]
    public async Task GetStockSummaryAsync_ShouldReturnEmpty_WhenNoStockItems()
    {
        var result = await _service.GetStockSummaryAsync(TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should group by variant across multiple variants")]
    public async Task GetStockSummaryAsync_ShouldGroupByMultipleVariants()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = v1, StockLocationId = loc, CountOnHand = 5 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = v2, StockLocationId = loc, CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _service.GetStockSummaryAsync(ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.VariantId).Should().Contain([v1, v2]);
    }

    #endregion

    #region GetStockAvailabilityAsync

    [Fact(DisplayName = "GetStockAvailabilityAsync: returns per-variant availability with location breakdown")]
    public async Task GetStockAvailabilityAsync_ReturnsPerVariantAvailability()
    {
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();
        var loc = Guid.NewGuid();
        SeedLocation(loc, "WH-1", active: true);
        SeedStockItemForVariant(v1, loc, onHand: 5);
        SeedStockItemForVariant(v2, loc, onHand: 2);
        SeedReservationForVariant(v1, loc, quantity: 1, expiresInMinutes: 30);

        var result = await _service.GetStockAvailabilityAsync(
            new[] { v1, v2 }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var avail1 = result.Value.Single(a => a.VariantId == v1);
        avail1.TotalOnHand.Should().Be(5);
        avail1.TotalReserved.Should().Be(1);
        avail1.TotalAvailable.Should().Be(4);
        avail1.Backorderable.Should().BeFalse();
        avail1.Locations.Should().HaveCount(1);

        var avail2 = result.Value.Single(a => a.VariantId == v2);
        avail2.TotalAvailable.Should().Be(2);
        avail2.Backorderable.Should().BeFalse();
    }

    [Fact(DisplayName = "GetStockAvailabilityAsync: returns default entry for variants with no stock")]
    public async Task GetStockAvailabilityAsync_ReturnsDefaultForNoStock()
    {
        var variantId = Guid.NewGuid();
        var result = await _service.GetStockAvailabilityAsync(
            new[] { variantId }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var entry = result.Value.Should().ContainSingle().Subject;
        entry.VariantId.Should().Be(variantId);
        entry.TotalOnHand.Should().Be(0);
        entry.TotalAvailable.Should().Be(0);
        entry.Backorderable.Should().BeFalse();
    }

    [Fact(DisplayName = "GetStockAvailabilityAsync: returns empty list for empty input")]
    public async Task GetStockAvailabilityAsync_ReturnsEmpty_ForEmptyInput()
    {
        var result = await _service.GetStockAvailabilityAsync([], TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetStockAvailabilityAsync: should handle duplicate variant ids without duplication")]
    public async Task GetStockAvailabilityAsync_ShouldDeduplicateVariantIds()
    {
        var variantId = Guid.NewGuid();
        var loc = Guid.NewGuid();
        SeedLocation(loc, "WH-1", active: true);
        SeedStockItemForVariant(variantId, loc, onHand: 5);

        var result = await _service.GetStockAvailabilityAsync(
            new[] { variantId, variantId }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GetStockAvailabilityAsync: should compute per-variant backorderable from any location")]
    public async Task GetStockAvailabilityAsync_ShouldComputeBackorderablePerVariant()
    {
        var variantId = Guid.NewGuid();
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        SeedLocation(locA, "LocA", active: true);
        SeedLocation(locB, "LocB", active: true);
        SeedStockItemForVariant(variantId, locA, onHand: 5, backorderable: false);
        SeedStockItemForVariant(variantId, locB, onHand: 5, backorderable: true);

        var result = await _service.GetStockAvailabilityAsync(
            new[] { variantId }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var entry = result.Value.Should().ContainSingle().Subject;
        entry.Backorderable.Should().BeTrue();
        entry.Locations.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetStockAvailabilityAsync: should exclude inactive or deleted location")]
    public async Task GetStockAvailabilityAsync_ShouldExcludeInactiveLocation()
    {
        var variantId = Guid.NewGuid();
        var locActive = Guid.NewGuid();
        var locInactive = Guid.NewGuid();
        SeedLocation(locActive, "Active", active: true);
        SeedLocation(locInactive, "Inactive", active: false);
        SeedStockItemForVariant(variantId, locActive, onHand: 5);
        SeedStockItemForVariant(variantId, locInactive, onHand: 20);

        var result = await _service.GetStockAvailabilityAsync(
            new[] { variantId }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var entry = result.Value.Should().ContainSingle().Subject;
        entry.Locations.Should().HaveCount(1);
        entry.Locations[0].StockLocationName.Should().Be("Active");
        entry.TotalOnHand.Should().Be(5);
    }

    #endregion

    #region Helpers

    private void SeedLocation(Guid id, string name, bool active)
    {
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = id, Name = name, Active = active });
        _dbContext.SaveChanges();
    }

    private void SeedStockItemForVariant(Guid variantId, Guid locationId, int onHand, bool backorderable = false)
    {
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = variantId, StockLocationId = locationId,
            CountOnHand = onHand, Backorderable = backorderable
        });
        _dbContext.SaveChanges();
    }

    private void SeedReservationForVariant(Guid variantId, Guid locationId, int quantity, int expiresInMinutes)
    {
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            Id = Guid.NewGuid(), VariantId = variantId, StockLocationId = locationId,
            Quantity = quantity, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes), CartToken = "test-cart"
        });
        _dbContext.SaveChanges();
    }

    #endregion
}
