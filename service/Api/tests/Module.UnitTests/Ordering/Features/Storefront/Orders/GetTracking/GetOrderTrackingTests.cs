using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Orders.GetTracking;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Storefront.Orders.GetTracking;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderTracking")]
public class GetOrderTrackingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public GetOrderTrackingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
    }

    [Fact(DisplayName = "Handler: Should populate EstimatedDeliveryAt from shipment")]
    public async Task Handle_WhenShipmentHasEstimate_ShouldPopulateEstimatedDeliveryAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", _userId).Value;
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        shipment.EstimatedDeliveryAtUtc = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetOrderTracking.QueryHandler(_dbContext, _currentUserMock.Object);
        var result = await handler.Handle(new GetOrderTracking.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstimatedDeliveryAt.Should().Be(shipment.EstimatedDeliveryAtUtc);
    }

    [Fact(DisplayName = "Handler: Should leave EstimatedDeliveryAt null when no shipment")]
    public async Task Handle_WhenNoShipment_ShouldLeaveEstimatedDeliveryAtNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", _userId).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetOrderTracking.QueryHandler(_dbContext, _currentUserMock.Object);
        var result = await handler.Handle(new GetOrderTracking.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstimatedDeliveryAt.Should().BeNull();
        result.Value.DeliveryExceptionAt.Should().BeNull();
    }

    public void Dispose() => _dbContext.Dispose();
}
