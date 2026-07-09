using Module.Inventory.Services;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockSummaryService")]
public class StockSummaryServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockSummaryService _service;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public StockSummaryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _service = new StockSummaryService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

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

        var result = await _service.GetStockSummaryAsync(ct);

        result.Should().HaveCount(1);
        result[0].LocationBreakdown[0].IsLowStock.Should().BeTrue();
    }

    [Fact(DisplayName = "GetStockSummaryAsync: Should return empty when no stock items")]
    public async Task GetStockSummaryAsync_ShouldReturnEmpty_WhenNoStockItems()
    {
        var result = await _service.GetStockSummaryAsync(TestContext.Current.CancellationToken);
        result.Should().BeEmpty();
    }
}
