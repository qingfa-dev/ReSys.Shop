using Microsoft.Extensions.Logging.Abstractions;

using Module.Ordering.Features.Storefront.RecordOrderShipmentState;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.UpdateStatus;
using Module.Shipping.Services;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.UpdateStatus;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "UpdateShipmentStatus")]
public class UpdateShipmentStatusTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly UpdateShipmentStatus.CommandHandler _handler;

    public UpdateShipmentStatusTests()
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

        var syncService = new ShipmentFulfillmentSyncService(
            _dbContext, _senderMock.Object, NullLogger<ShipmentFulfillmentSyncService>.Instance);

        _handler = new UpdateShipmentStatus.CommandHandler(_dbContext, syncService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: marks a Ready shipment Shipped with a tracking number and syncs the order")]
    public async Task Handle_ShouldMarkShipped_AndSyncOrder()
    {
        var shipment = ShipmentMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        shipment.Status = ShipmentStatus.Ready;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateShipmentStatus.Command(
                shipment.Id,
                new UpdateShipmentStatus.Request { Status = ShipmentStatus.Shipped, TrackingNumber = "TRK123" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Shipment>().FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        persisted.Status.Should().Be(ShipmentStatus.Shipped);
        persisted.TrackingNumber.Should().Be("TRK123");

        _senderMock.Verify(s => s.Send(
            It.Is<RecordOrderShipmentStateCommand>(c => c.OrderId == shipment.OrderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle: returns invalid transition when skipping states")]
    public async Task Handle_ShouldReturnInvalidTransition_WhenSkippingStates()
    {
        var shipment = ShipmentMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateShipmentStatus.Command(
                shipment.Id,
                new UpdateShipmentStatus.Request { Status = ShipmentStatus.Delivered }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Shipment.InvalidStateTransition");
    }

    [Fact(DisplayName = "Handle: returns not found when shipment is missing")]
    public async Task Handle_ShouldReturnNotFound_WhenShipmentMissing()
    {
        var result = await _handler.Handle(
            new UpdateShipmentStatus.Command(
                Guid.NewGuid(),
                new UpdateShipmentStatus.Request { Status = ShipmentStatus.Shipped, TrackingNumber = "TRK1" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Shipment.NotFound");
    }
}
