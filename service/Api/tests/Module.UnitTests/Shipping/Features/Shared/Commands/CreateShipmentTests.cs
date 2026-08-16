using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Shared.Commands;

namespace Module.UnitTests.Shipping.Features.Shared.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CreateShipment")]
public class CreateShipmentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateShipmentCommandHandler _handler;

    public CreateShipmentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateShipmentCommandHandler(_dbContext);
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
}
