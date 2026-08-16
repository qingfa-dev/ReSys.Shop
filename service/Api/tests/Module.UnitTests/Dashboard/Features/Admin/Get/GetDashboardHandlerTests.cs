using Module.Catalog.Domain.Products;
using Module.Dashboard.Features.Admin.Get;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Dashboard.Features.Admin.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Dashboard")]
[Trait("Feature", "GetDashboard")]
public class GetDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetDashboard.QueryHandler _handler;

    public GetDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros and empty lists when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Sales.TotalRevenue.Should().Be(0m);
        response.Sales.OrderCount.Should().Be(0);
        response.Sales.AverageOrderValue.Should().Be(0m);
        response.Sales.RevenueTrendPercentage.Should().Be(0m);
        response.Sales.TrendHistory.Should().HaveCount(30);
        response.Sales.TrendHistory.Should().AllSatisfy(p => p.Revenue.Should().Be(0m));
        response.Inventory.TotalVariants.Should().Be(0);
        response.Inventory.OutOfStockCount.Should().Be(0);
        response.Inventory.LowStockCount.Should().Be(0);
        response.Catalog.TotalProducts.Should().Be(0);
        response.Catalog.ActiveProducts.Should().Be(0);
        response.Catalog.RecentlyAdded.Should().BeEmpty();
        response.RecentActivities.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should compute correct sales totals from placed orders")]
    public async Task Handle_ShouldComputeSales_WhenOrdersExist()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 2,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Placed,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-003",
            Status = OrderStatus.Canceled,
            Total = 99m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var sales = result.Value.Sales;
        sales.TotalRevenue.Should().Be(300m);
        sales.OrderCount.Should().Be(2);
        sales.AverageOrderValue.Should().Be(150m);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted and draft orders from counts")]
    public async Task Handle_ShouldExcludeDeletedAndDraftOrders()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Draft,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-003",
            Status = OrderStatus.Placed,
            Total = 300m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sales.OrderCount.Should().Be(1);
        result.Value.Sales.TotalRevenue.Should().Be(100m);
    }

    [Fact(DisplayName = "Handle: Should count active products, not draft or archived")]
    public async Task Handle_ShouldCountActiveProductsOnly()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Active Product",
            Slug = "active-product",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Draft Product",
            Slug = "draft-product",
            Status = ProductStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Archived Product",
            Slug = "archived-product",
            Status = ProductStatus.Archived,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var catalog = result.Value.Catalog;
        catalog.TotalProducts.Should().Be(3);
        catalog.ActiveProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return recent activities interleaved from orders and stock")]
    public async Task Handle_ShouldReturnInterleavedActivities()
    {
        var ct = TestContext.Current.CancellationToken;

        var orderId = Guid.NewGuid();
        _dbContext.Set<Order>().Add(new Order
        {
            Id = orderId,
            Number = "ORD-100",
            Status = OrderStatus.Placed,
            Total = 50m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-1)
        });

        var locId = Guid.NewGuid();
        var siId = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId,
            Name = "Warehouse A",
            Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = siId,
            VariantId = Guid.NewGuid(),
            StockLocationId = locId,
            CountOnHand = 10
        });
        _dbContext.Set<StockMovement>().Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            StockItemId = siId,
            Quantity = 5,
            Action = "restock",
            Reason = "Shipment received",
            CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-2)
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var activities = result.Value.RecentActivities;
        activities.Should().NotBeEmpty();
        activities.Should().Contain(a => a.Type == "Order");
        activities.Should().Contain(a => a.Type == "Stock");
        activities.Should().BeInDescendingOrder(a => a.Timestamp);
    }

    [Fact(DisplayName = "Handle: TrendHistory should have exactly 30 data points")]
    public async Task Handle_TrendHistory_ShouldHave30Points()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-15)
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var trend = result.Value.Sales.TrendHistory;
        trend.Should().HaveCount(30);
        trend.Should().BeInAscendingOrder(p => p.Date);
        trend.Should().ContainSingle(p => p.Revenue == 100m);
    }
}
