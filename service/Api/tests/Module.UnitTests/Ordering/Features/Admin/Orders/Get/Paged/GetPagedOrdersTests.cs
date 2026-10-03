using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.Paged;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetPagedOrders")]
public class GetPagedOrdersTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedOrders.PagedQueryHandler _handler;

    public GetPagedOrdersTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPagedOrders.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return empty list when no orders exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoOrders()
    {
        var result = await _handler.Handle(
            new GetPagedOrders.Query(new GetPagedOrders.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should return orders when they exist")]
    public async Task Handle_ShouldReturnOrders_WhenTheyExist()
    {
        var order1 = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        var order2 = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        _dbContext.Set<Order>().AddRange(order1, order2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPagedOrders.Query(new GetPagedOrders.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Handler: Should filter orders by status")]
    public async Task Handle_ShouldFilterByStatus()
    {
        var placed = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        placed.Status = OrderStatus.Placed;
        var draft = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        _dbContext.Set<Order>().AddRange(placed, draft);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPagedOrders.Query(new GetPagedOrders.Parameters { Filter = "Status=Placed" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be(OrderStatus.Placed);
    }
}
