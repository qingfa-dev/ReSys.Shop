using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.ShipmentById;
using Module.Shipping.Domain.Shipments;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.ShipmentById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderShipmentById")]
public class GetOrderShipmentByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderShipmentById.QueryHandler _handler;

    public GetOrderShipmentByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly, typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderShipmentById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return shipment when found")]
    public async Task Handle_ShouldReturnShipment_WhenFound()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shipment = ShipmentExtensions.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Number = "SHIP-001";
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderShipmentById.Query(order.Id, shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(shipment.Id);
        result.Value.Number.Should().Be("SHIP-001");
    }

    [Fact(DisplayName = "Handler: Should return not found when shipment missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderShipmentById.Query(order.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(ShipmentResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
