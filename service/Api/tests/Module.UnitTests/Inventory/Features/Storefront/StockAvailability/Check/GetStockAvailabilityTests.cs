using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.StockAvailability.Check;

namespace Module.UnitTests.Inventory.Features.Storefront.StockAvailability;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockAvailabilityCheck")]
public class GetStockAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockAvailability.QueryHandler _handler;

    public GetStockAvailabilityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetStockAvailability.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return availability for variant across locations")]
    public async Task Handle_ShouldReturnAvailability_WhenVariantHasStock()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location1 = StockLocationMethod.Create("Warehouse A", city: "NYC").Value;
        var location2 = StockLocationMethod.Create("Warehouse B", city: "LAX").Value;
        _dbContext.Set<StockLocation>().AddRange(location1, location2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<StockItem>().AddRange(
            new StockItem { VariantId = variantId, StockLocationId = location1.Id, CountOnHand = 5, Backorderable = false },
            new StockItem { VariantId = variantId, StockLocationId = location2.Id, CountOnHand = 3, Backorderable = true }
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalOnHand.Should().Be(8);
        result.Value.TotalReserved.Should().Be(0);
        result.Value.TotalAvailable.Should().Be(8);
        result.Value.LocationAvailability.Should().HaveCount(2);
        result.Value.LocationAvailability.Sum(x => x.CountOnHand).Should().Be(8);
    }

    [Fact(DisplayName = "Handler: Should return zero availability when no stock exists")]
    public async Task Handle_ShouldReturnZero_WhenNoStock()
    {
        // Arrange
        var variantId = Guid.NewGuid();

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalOnHand.Should().Be(0);
        result.Value.TotalAvailable.Should().Be(0);
        result.Value.LocationAvailability.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should subtract active reservations from available count")]
    public async Task Handle_ShouldSubtractActiveReservations()
    {
        // Arrange: 10 on hand, 3 reserved active
        var variantId = Guid.NewGuid();
        var location1 = StockLocationMethod.Create("Warehouse A").Value;
        _dbContext.Set<StockLocation>().Add(location1);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<StockItem>().Add(
            new StockItem { VariantId = variantId, StockLocationId = location1.Id, CountOnHand = 10 }
        );
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = variantId,
            StockLocationId = location1.Id,
            OrderId = Guid.NewGuid(),
            Quantity = 3,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalOnHand.Should().Be(10);
        result.Value.TotalReserved.Should().Be(3);
        result.Value.TotalAvailable.Should().Be(7);
    }

    [Fact(DisplayName = "Handler: Should show per-location reserved and available counts")]
    public async Task Handle_ShouldShowPerLocationReservedAndAvailable()
    {
        // Arrange: LocationA=5 on hand + 2 reserved → available=3
        var variantId = Guid.NewGuid();
        var locA = StockLocationMethod.Create("Warehouse A").Value;
        _dbContext.Set<StockLocation>().Add(locA);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<StockItem>().Add(
            new StockItem { VariantId = variantId, StockLocationId = locA.Id, CountOnHand = 5 }
        );
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = variantId,
            StockLocationId = locA.Id,
            Quantity = 2,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert
        var loc = result.Value.LocationAvailability.Should().ContainSingle().Subject;
        loc.CountOnHand.Should().Be(5);
        loc.ReservedCount.Should().Be(2);
        loc.AvailableCount.Should().Be(3);
    }

    [Fact(DisplayName = "Handler: Should ignore expired reservations")]
    public async Task Handle_ShouldIgnoreExpiredReservations()
    {
        // Arrange: 10 on hand, 3 expired → available=10
        var variantId = Guid.NewGuid();
        var loc = StockLocationMethod.Create("Warehouse A").Value;
        _dbContext.Set<StockLocation>().Add(loc);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<StockItem>().Add(
            new StockItem { VariantId = variantId, StockLocationId = loc.Id, CountOnHand = 10 }
        );
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = variantId,
            StockLocationId = loc.Id,
            Quantity = 3,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(-10), // expired
            CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-40)
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.TotalAvailable.Should().Be(10); // not reduced by expired
        result.Value.TotalReserved.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: AvailableCount should never be negative")]
    public async Task Handle_ShouldNotReturnNegativeAvailableCount()
    {
        // Arrange: 0 on hand but 3 reserved (should clamp to 0)
        var variantId = Guid.NewGuid();
        var loc = StockLocationMethod.Create("Warehouse A").Value;
        _dbContext.Set<StockLocation>().Add(loc);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<StockItem>().Add(
            new StockItem { VariantId = variantId, StockLocationId = loc.Id, CountOnHand = 0 }
        );
        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = variantId,
            StockLocationId = loc.Id,
            Quantity = 3,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new GetStockAvailability.Query(variantId),
            TestContext.Current.CancellationToken);

        // Assert: AvailableCount should be 0, not -3
        var locAvailability = result.Value.LocationAvailability.Should().ContainSingle().Subject;
        locAvailability.AvailableCount.Should().Be(0);
        result.Value.TotalAvailable.Should().Be(0);
    }
}
