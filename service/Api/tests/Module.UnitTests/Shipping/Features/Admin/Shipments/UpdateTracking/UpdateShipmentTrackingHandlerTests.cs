using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.UpdateTracking;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.UpdateTracking;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "UpdateShipmentTracking")]
public class UpdateShipmentTrackingHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateShipmentTracking.CommandHandler>> _loggerMock;
    private readonly UpdateShipmentTracking.CommandHandler _handler;

    public UpdateShipmentTrackingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateShipmentTracking.CommandHandler>>();
        _handler = new UpdateShipmentTracking.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update tracking when shipment found")]
    public async Task Handle_ShouldUpdateTracking_WhenShipmentFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateShipmentTracking.Request { Tracking = "NEWTRACK" };
        var result = await _handler.Handle(
            new UpdateShipmentTracking.Command(shipment.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tracking.Should().Be("NEWTRACK");

        var saved = await _dbContext.Set<Shipment>()
            .FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        saved.Tracking.Should().Be("NEWTRACK");
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var request = new UpdateShipmentTracking.Request { Tracking = "NEWTRACK" };
        var result = await _handler.Handle(
            new UpdateShipmentTracking.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}