using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Create;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CreateShipment")]
public class CreateShipmentHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CreateShipment.CommandHandler>> _loggerMock;
    private readonly CreateShipment.CommandHandler _handler;

    public CreateShipmentHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateShipment.CommandHandler>>();
        _handler = new CreateShipment.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create shipment when valid request")]
    public async Task Handle_ShouldCreateShipment_WhenValidRequest()
    {
        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var orderId = Guid.NewGuid();
        var stockLocationId = Guid.NewGuid();
        var request = new CreateShipment.Request
        {
            Number = "SHP-001",
            OrderId = orderId,
            StockLocationId = stockLocationId,
            ShippingMethodId = method.Id,
            Cost = 5.99m
        };

        var result = await _handler.Handle(
            new CreateShipment.Command(request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.ShippingMethodId.Should().Be(method.Id);

        var saved = await _dbContext.Set<Shipment>()
            .FirstAsync(s => s.OrderId == orderId, TestContext.Current.CancellationToken);
        saved.ShippingMethodId.Should().Be(method.Id);
    }

    [Fact(DisplayName = "Handler: Should persist shipment to database")]
    public async Task Handle_ShouldPersistShipment_ToDatabase()
    {
        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateShipment.Request
        {
            OrderId = Guid.NewGuid(),
            StockLocationId = Guid.NewGuid(),
            ShippingMethodId = method.Id,
            Cost = 9.99m
        };

        var result = await _handler.Handle(
            new CreateShipment.Command(request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var count = await _dbContext.Set<Shipment>().CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(1);
    }
}