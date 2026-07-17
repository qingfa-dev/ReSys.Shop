using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Ordering.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderingDashboard")]
public class GetOrderingDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderingDashboard.QueryHandler _handler;

    public GetOrderingDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderingDashboard.QueryHandler(_dbContext);
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

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalOrders.Should().Be(0);
        response.TotalRevenue.Should().Be(0m);
        response.AverageOrderValue.Should().Be(0m);
        response.PendingFulfillment.Should().Be(0);
        response.TodayOrders.Should().Be(0);
        response.RecentOrders.Should().BeEmpty();
        response.StatusBreakdown.Draft.Should().Be(0);
        response.StatusBreakdown.Placed.Should().Be(0);
        response.StatusBreakdown.Canceled.Should().Be(0);
        response.StatusBreakdown.Expired.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should compute order counts and revenue correctly")]
    public async Task Handle_ShouldComputeOrderCounts_WhenOrdersExist()
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
            Status = OrderStatus.Draft,
            Total = 50m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalOrders.Should().Be(3);
        response.TotalRevenue.Should().Be(350m);
        response.AverageOrderValue.Should().BeApproximately(116.67m, 0.01m);
        response.PendingFulfillment.Should().Be(2);
        response.TodayOrders.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted orders")]
    public async Task Handle_ShouldExcludeDeletedOrders()
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
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Placed,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalOrders.Should().Be(1);
        result.Value.TotalRevenue.Should().Be(100m);
    }

    [Fact(DisplayName = "Handle: Status breakdown should count each status correctly")]
    public async Task Handle_StatusBreakdown_ShouldCountEachStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-1", Status = OrderStatus.Draft,
            Total = 10m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-2", Status = OrderStatus.Placed,
            Total = 20m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-3", Status = OrderStatus.Placed,
            Total = 30m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-4", Status = OrderStatus.Canceled,
            Total = 40m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var bd = result.Value.StatusBreakdown;
        bd.Draft.Should().Be(1);
        bd.Placed.Should().Be(2);
        bd.Canceled.Should().Be(1);
        bd.Expired.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should return top 10 recent orders")]
    public async Task Handle_ShouldReturnRecentOrders()
    {
        var ct = TestContext.Current.CancellationToken;
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 1; i <= 12; i++)
        {
            _dbContext.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Number = $"ORD-{i:D3}",
                Status = OrderStatus.Placed,
                Total = i * 10m,
                Currency = "USD",
                ItemCount = 1,
                CreatedAtUtc = baseTime.AddHours(-i)
            });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var recent = result.Value.RecentOrders;
        recent.Should().HaveCount(10);
        recent.Should().BeInDescendingOrder(o => o.CreatedAtUtc);
    }
}
