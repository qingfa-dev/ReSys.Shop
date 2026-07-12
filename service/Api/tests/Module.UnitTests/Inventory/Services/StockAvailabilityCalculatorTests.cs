using Microsoft.EntityFrameworkCore;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
public class StockAvailabilityCalculatorTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly StockAvailabilityCalculator _sut;

    public StockAvailabilityCalculatorTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _db = new ApplicationDbContext(opts);
        _sut = new StockAvailabilityCalculator(_db);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "GetForVariant: no stock returns zeros and not backorderable")]
    public async Task GetForVariant_NoStock_ReturnsZero()
    {
        var variantId = Guid.NewGuid();
        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.TotalOnHand.Should().Be(0);
        result.TotalReserved.Should().Be(0);
        result.TotalAvailable.Should().Be(0);
        result.Backorderable.Should().BeFalse();
        result.Locations.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetForVariant: excludes reservations that have expired")]
    public async Task GetForVariant_ExcludesExpiredReservations()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SeedLocation(locationId, "WH-1", active: true);
        SeedStockItem(variantId, locationId, onHand: 10, backorderable: false);
        SeedReservation(variantId, locationId, quantity: 5, expiresInMinutes: -10); // expired

        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.TotalReserved.Should().Be(0);
        result.TotalAvailable.Should().Be(10);
    }

    [Fact(DisplayName = "GetForVariant: any backorderable location makes the variant backorderable")]
    public async Task GetForVariant_AnyBackorderableLocation_IsBackorderable()
    {
        var variantId = Guid.NewGuid();
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        SeedLocation(locA, "WH-A", active: true);
        SeedLocation(locB, "WH-B", active: true);
        SeedStockItem(variantId, locA, onHand: 0, backorderable: false);
        SeedStockItem(variantId, locB, onHand: 0, backorderable: true);

        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "GetAvailableByVariant: returns map of variant id to available count")]
    public async Task GetAvailableByVariant_ReturnsMap()
    {
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();
        var loc = Guid.NewGuid();
        SeedLocation(loc, "WH-1", active: true);
        SeedStockItem(v1, loc, onHand: 5, backorderable: false);
        SeedStockItem(v2, loc, onHand: 2, backorderable: false);
        SeedReservation(v1, loc, quantity: 1, expiresInMinutes: 30);

        var result = await _sut.GetAvailableByVariantAsync(
            new[] { v1, v2 }, TestContext.Current.CancellationToken);

        result[v1].Should().Be(4);
        result[v2].Should().Be(2);
    }

    private void SeedLocation(Guid id, string name, bool active)
    {
        _db.Set<StockLocation>().Add(new StockLocation { Id = id, Name = name, Active = active });
        _db.SaveChanges();
    }

    private void SeedStockItem(Guid variantId, Guid locationId, int onHand, bool backorderable)
    {
        _db.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            StockLocationId = locationId,
            CountOnHand = onHand,
            Backorderable = backorderable
        });
        _db.SaveChanges();
    }

    private void SeedReservation(Guid variantId, Guid locationId, int quantity, int expiresInMinutes)
    {
        _db.Set<StockReservation>().Add(new StockReservation
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            StockLocationId = locationId,
            Quantity = quantity,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes),
            CartToken = "test-cart"
        });
        _db.SaveChanges();
    }
}
