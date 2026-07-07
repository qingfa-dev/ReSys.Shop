using BuildingBlocks.Querying.Models;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Orders.ListOrders;

namespace Module.UnitTests.Ordering.Features.Storefront.Orders.ListOrders;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ListCustomerOrders")]
public class ListCustomerOrdersTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ListCustomerOrders.PagedQueryHandler _handler;
    private readonly Guid _userId;

    public ListCustomerOrdersTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new ListCustomerOrders.PagedQueryHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should list customer orders")]
    public async Task Handle_ShouldReturnOrders_WhenOrdersExist()
    {
        var cart1 = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        cart1.Number = "R20260520-ABC123";
        cart1.Status = OrderStatus.Placed;

        var cart2 = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        cart2.Number = "R20260520-DEF456";
        cart2.Status = OrderStatus.Placed;

        _dbContext.Set<Order>().AddRange(cart1, cart2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ListCustomerOrders.Query(new QueryingParameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty when no orders")]
    public async Task Handle_ShouldReturnEmpty_WhenNoOrders()
    {
        var result = await _handler.Handle(new ListCustomerOrders.Query(new QueryingParameters()), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty when user not authenticated")]
    public async Task Handle_ShouldReturnEmpty_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);
        var result = await _handler.Handle(new ListCustomerOrders.Query(new QueryingParameters()), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
