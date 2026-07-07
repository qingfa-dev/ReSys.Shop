using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Cancel;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Cancel;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CancelShipment")]
public class CancelShipmentHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CancelShipment.CommandHandler>> _loggerMock;
    private readonly CancelShipment.CommandHandler _handler;

    public CancelShipmentHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CancelShipment.CommandHandler>>();
        _handler = new CancelShipment.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should cancel shipment when found")]
    public async Task Handle_ShouldCancelShipment_WhenFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CancelShipment.Command(shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<Shipment>()
            .FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        saved.State.Should().Be(ShipmentState.Canceled);
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new CancelShipment.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}