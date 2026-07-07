using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Ship;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Ship;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "MarkShipmentShipped")]
public class MarkShipmentShippedHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<MarkShipmentShipped.CommandHandler>> _loggerMock;
    private readonly MarkShipmentShipped.CommandHandler _handler;

    public MarkShipmentShippedHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly, typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<MarkShipmentShipped.CommandHandler>>();
        _handler = new MarkShipmentShipped.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should mark shipped when all conditions met")]
    public async Task Handle_ShouldMarkShipped_WhenAllConditionsMet()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Email = "test@example.com";
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shipment = ShipmentExtensions.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Ready();
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new MarkShipmentShipped.Request { Tracking = "TRACK123" };
        var result = await _handler.Handle(
            new MarkShipmentShipped.Command(shipment.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tracking.Should().Be("TRACK123");
        result.Value.State.Should().Be(ShipmentState.Shipped);
        result.Value.ShippedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return not found when shipment missing")]
    public async Task Handle_ShouldReturnNotFound_WhenShipmentMissing()
    {
        var request = new MarkShipmentShipped.Request { Tracking = "TRACK123" };
        var result = await _handler.Handle(
            new MarkShipmentShipped.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}