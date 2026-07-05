using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;
using Module.Inventory.Features.Admin.StockItems.Summary;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Summary;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockSummary")]
public class GetStockSummaryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockSummary.QueryHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public GetStockSummaryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStockSummary.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return consolidated per-variant totals")]
    public async Task Handle_ShouldReturnConsolidatedPerVariant()
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

        _dbContext.Set<StockReservation>().Add(new StockReservation
        {
            VariantId = _variantId, StockLocationId = locA, OrderId = _orderId,
            Quantity = 2, State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30), CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetStockSummary.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var summary = result.Value[0];
        summary.VariantId.Should().Be(_variantId);
        summary.TotalOnHand.Should().Be(12);
        summary.TotalReserved.Should().Be(2);
        summary.TotalAvailable.Should().Be(10);
        summary.LocationBreakdown.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should flag low stock items")]
    public async Task Handle_ShouldFlagLowStockItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = _variantId, StockLocationId = loc, CountOnHand = 3 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetStockSummary.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].LocationBreakdown[0].IsLowStock.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return empty when no stock items")]
    public async Task Handle_ShouldReturnEmpty_WhenNoStockItems()
    {
        var result = await _handler.Handle(new GetStockSummary.Query(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
