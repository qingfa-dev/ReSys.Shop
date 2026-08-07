using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.CheckVariantAvailability;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Features.Storefront.Contracts.CheckVariantAvailability;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CheckVariantAvailability")]
public class CheckVariantAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CheckVariantAvailabilityQueryHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _locationId = Guid.NewGuid();

    public CheckVariantAvailabilityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        var availabilityService = new StockAvailabilityService(_dbContext);
        _handler = new CheckVariantAvailabilityQueryHandler(availabilityService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CheckVariantAvailability: Should return available when stock is sufficient")]
    public async Task Handle_ShouldReturnAvailable_WhenStockSufficient()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckVariantAvailabilityQuery(_variantId, 5),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeTrue();
        result.Value.VariantId.Should().Be(_variantId);
    }

    [Fact(DisplayName = "CheckVariantAvailability: Should return unavailable when stock insufficient")]
    public async Task Handle_ShouldReturnUnavailable_WhenStockInsufficient()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 3).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckVariantAvailabilityQuery(_variantId, 10),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact(DisplayName = "CheckVariantAvailability: Should return available for zero quantity")]
    public async Task Handle_ShouldReturnAvailable_WhenQuantityZero()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(
            new CheckVariantAvailabilityQuery(_variantId, 0),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeTrue();
    }

    [Fact(DisplayName = "CheckVariantAvailability: Should subtract active reservations")]
    public async Task Handle_ShouldSubtractActiveReservations()
    {
        var ct = TestContext.Current.CancellationToken;

        var stockItem = StockItemMethod.Create(stockLocationId: _locationId, variantId: _variantId, countOnHand: 5).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity: 3, state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(10),
            stockLocationId: _locationId, orderId: Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CheckVariantAvailabilityQuery(_variantId, 3),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact(DisplayName = "CheckVariantAvailability: Should return unavailable when variant has no stock items")]
    public async Task Handle_ShouldReturnUnavailable_WhenNoStockItems()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(
            new CheckVariantAvailabilityQuery(Guid.NewGuid(), 1),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }
}
