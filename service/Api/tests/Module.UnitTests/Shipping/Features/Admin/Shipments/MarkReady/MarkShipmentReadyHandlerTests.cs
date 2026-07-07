using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.MarkReady;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.MarkReady;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "MarkShipmentReady")]
public class MarkShipmentReadyHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<MarkShipmentReady.CommandHandler>> _loggerMock;
    private readonly MarkShipmentReady.CommandHandler _handler;

    public MarkShipmentReadyHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<MarkShipmentReady.CommandHandler>>();
        _handler = new MarkShipmentReady.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should mark ready when shipment found")]
    public async Task Handle_ShouldMarkReady_WhenShipmentFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        shipment.Pend();
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new MarkShipmentReady.Command(shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<Shipment>()
            .FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        saved.State.Should().Be(ShipmentState.Ready);
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new MarkShipmentReady.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}