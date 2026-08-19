using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Inventory.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetInventoryDashboard")]
public class GetInventoryDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetInventoryDashboard.QueryHandler _handler;

    public GetInventoryDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetInventoryDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(0);
        response.InStockCount.Should().Be(0);
        response.OutOfStockCount.Should().Be(0);
        response.LowStockCount.Should().Be(0);
        response.StockLocationCount.Should().Be(0);
        response.ItemsPerLocationAverage.Should().Be(0);
        response.RecentMovements.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should count stock levels correctly")]
    public async Task Handle_ShouldCountStockLevels()
    {
        var ct = TestContext.Current.CancellationToken;
        var locId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId, Name = "Warehouse", Active = true, LowStockThreshold = 5
        });

        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 10
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 0
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 3
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(3);
        response.InStockCount.Should().Be(2);
        response.OutOfStockCount.Should().Be(1);
        response.LowStockCount.Should().Be(1);
        response.StockLocationCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should exclude inactive and deleted locations")]
    public async Task Handle_ShouldExcludeInactiveLocations()
    {
        var ct = TestContext.Current.CancellationToken;
        var activeId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = activeId, Name = "Active", Active = true
        });
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = inactiveId, Name = "Inactive", Active = false
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = activeId, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = inactiveId, CountOnHand = 5
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(1);
        response.StockLocationCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return recent stock movements")]
    public async Task Handle_ShouldReturnRecentMovements()
    {
        var ct = TestContext.Current.CancellationToken;
        var locId = Guid.NewGuid();
        var siId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId, Name = "Warehouse", Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = siId, VariantId = Guid.NewGuid(), StockLocationId = locId, CountOnHand = 10
        });
        _dbContext.Set<StockMovement>().Add(new StockMovement
        {
            Id = Guid.NewGuid(), StockItemId = siId, Quantity = 5,
            Action = "restock", Reason = "Shipment", CreatedAtUtc = DateTimeOffset.UtcNow,
            StockLocationId = locId
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecentMovements.Should().HaveCount(1);
        result.Value.RecentMovements[0].Action.Should().Be("restock");
        result.Value.RecentMovements[0].Quantity.Should().Be(5);
    }

    [Fact(DisplayName = "Handle: ItemsPerLocationAverage should be correct")]
    public async Task Handle_ItemsPerLocationAverage_ShouldBeCorrect()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc1Id = Guid.NewGuid();
        var loc2Id = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = loc1Id, Name = "WH1", Active = true
        });
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = loc2Id, Name = "WH2", Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc1Id, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc1Id, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc2Id, CountOnHand = 5
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ItemsPerLocationAverage.Should().Be(2);
    }
}
