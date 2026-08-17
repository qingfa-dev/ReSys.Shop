using Microsoft.Extensions.Logging.Abstractions;

using Module.Ordering.Features.Storefront.RecordOrderShipmentState;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Shared.Commands;
using Module.Shipping.Services;

using Shared.Application.Domain.Orders;

namespace Module.UnitTests.Shipping.Features.Shared.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CreateShipment")]
public class CreateShipmentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly ShipmentFulfillmentSyncService _syncService;
    private readonly CreateShipmentCommandHandler _handler;

    public CreateShipmentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(s => s.Send(It.IsAny<RecordOrderShipmentStateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _syncService = new ShipmentFulfillmentSyncService(
            _dbContext, _senderMock.Object, NullLogger<ShipmentFulfillmentSyncService>.Instance);
        _handler = new CreateShipmentCommandHandler(_dbContext, _syncService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: creates a Pending shipment for the order and shipping method")]
    public async Task Handle_ShouldCreatePendingShipment()
    {
        var orderId = Guid.NewGuid();
        var shippingMethodId = Guid.NewGuid();

        var result = await _handler.Handle(
            new CreateShipmentCommand { OrderId = orderId, ShippingMethodId = shippingMethodId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var shipment = await _dbContext.Set<Shipment>().SingleAsync(TestContext.Current.CancellationToken);
        shipment.OrderId.Should().Be(orderId);
        shipment.ShippingMethodId.Should().Be(shippingMethodId);
        shipment.Status.Should().Be(ShipmentStatus.Pending);
    }

    [Fact(DisplayName = "Handle: does not create a duplicate shipment for the same order and method")]
    public async Task Handle_ShouldNotCreateDuplicate_WhenShipmentExists()
    {
        var orderId = Guid.NewGuid();
        var shippingMethodId = Guid.NewGuid();

        _dbContext.Set<Shipment>().Add(ShipmentMethod.Create(orderId, shippingMethodId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CreateShipmentCommand { OrderId = orderId, ShippingMethodId = shippingMethodId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _dbContext.Set<Shipment>().Count().Should().Be(1);
    }

    [Fact(DisplayName = "Handle: syncs order fulfillment state after creating a shipment")]
    public async Task Handle_ShouldSyncOrderFulfillment_AfterCreatingShipment()
    {
        var orderId = Guid.NewGuid();
        var shippingMethodId = Guid.NewGuid();

        var result = await _handler.Handle(
            new CreateShipmentCommand { OrderId = orderId, ShippingMethodId = shippingMethodId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _senderMock.Verify(s => s.Send(
            It.Is<RecordOrderShipmentStateCommand>(c => c.OrderId == orderId && c.FulfillmentState == ShipmentState.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
