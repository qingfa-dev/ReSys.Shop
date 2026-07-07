using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Update.Shipment;
using Module.Shipping.Domain.Shipments;
using ShipmentDomain = Module.Shipping.Domain.Shipments.Shipment;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Update.Shipment;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderShipment")]
public class UpdateOrderShipmentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateOrderShipment.CommandHandler _handler;

    public UpdateOrderShipmentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly, typeof(ShipmentDomain).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateOrderShipment.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update tracking number")]
    public async Task Handle_ShouldUpdateTracking()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shipment = ShipmentExtensions.Create(order.Id, Guid.NewGuid()).Value;
        _dbContext.Set<ShipmentDomain>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateOrderShipment.Command(order.Id, shipment.Id, new UpdateOrderShipment.Request
            {
                Tracking = "TRACK-12345"
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tracking.Should().Be("TRACK-12345");
    }

    [Fact(DisplayName = "Handler: Should update shipping method")]
    public async Task Handle_ShouldUpdateShippingMethod()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shipment = ShipmentExtensions.Create(order.Id, Guid.NewGuid()).Value;
        _dbContext.Set<ShipmentDomain>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newMethodId = Guid.NewGuid();
        var result = await _handler.Handle(
            new UpdateOrderShipment.Command(order.Id, shipment.Id, new UpdateOrderShipment.Request
            {
                ShippingMethodId = newMethodId
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingMethodId.Should().Be(newMethodId);
    }

    [Fact(DisplayName = "Handler: Should return not found when shipment missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateOrderShipment.Command(order.Id, Guid.NewGuid(), new UpdateOrderShipment.Request()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(ShipmentResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
