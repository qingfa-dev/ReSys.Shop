using Microsoft.Extensions.Logging.Abstractions;

using Module.Ordering.Features.Storefront.RecordOrderShipmentState;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Services;

using Shared.Application.Domain.Orders;

namespace Module.UnitTests.Shipping.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShipmentFulfillmentSyncService")]
public class ShipmentFulfillmentSyncServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly ShipmentFulfillmentSyncService _service;

    public ShipmentFulfillmentSyncServiceTests()
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

        _service = new ShipmentFulfillmentSyncService(
            _dbContext, _senderMock.Object, NullLogger<ShipmentFulfillmentSyncService>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "SyncOrderFulfillmentAsync: sends Shipped state when one shipment is shipped")]
    public async Task SyncOrderFulfillmentAsync_ShouldSendShippedState_WhenOneShippedShipment()
    {
        var orderId = Guid.NewGuid();
        var shipment = ShipmentMethod.Create(orderId, Guid.NewGuid()).Value;
        shipment.Status = ShipmentStatus.Shipped;
        shipment.ShippedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _service.SyncOrderFulfillmentAsync(orderId, TestContext.Current.CancellationToken);

        _senderMock.Verify(s => s.Send(
            It.Is<RecordOrderShipmentStateCommand>(c => c.OrderId == orderId && c.FulfillmentState == ShipmentState.Shipped && c.ShippedAtUtc.HasValue),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SyncOrderFulfillmentAsync: sends None state when order has no shipments")]
    public async Task SyncOrderFulfillmentAsync_ShouldSendNone_WhenNoShipments()
    {
        var orderId = Guid.NewGuid();

        await _service.SyncOrderFulfillmentAsync(orderId, TestContext.Current.CancellationToken);

        _senderMock.Verify(s => s.Send(
            It.Is<RecordOrderShipmentStateCommand>(c => c.OrderId == orderId && c.FulfillmentState == ShipmentState.None),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SyncOrderFulfillmentAsync: does not throw when sender fails")]
    public async Task SyncOrderFulfillmentAsync_ShouldNotThrow_WhenSenderFails()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RecordOrderShipmentStateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Ordering.NotFound", "order not found")));

        var orderId = Guid.NewGuid();
        var shipment = ShipmentMethod.Create(orderId, Guid.NewGuid()).Value;
        shipment.Status = ShipmentStatus.Shipped;
        shipment.ShippedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = () => _service.SyncOrderFulfillmentAsync(orderId, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }
}
