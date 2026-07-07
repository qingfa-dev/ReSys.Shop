using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.MarkPending;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.MarkPending;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "MarkShipmentPending")]
public class MarkShipmentPendingHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<MarkShipmentPending.CommandHandler>> _loggerMock;
    private readonly MarkShipmentPending.CommandHandler _handler;

    public MarkShipmentPendingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<MarkShipmentPending.CommandHandler>>();
        _handler = new MarkShipmentPending.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should mark pending when shipment found")]
    public async Task Handle_ShouldMarkPending_WhenShipmentFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        shipment.Ready();
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new MarkShipmentPending.Command(shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<Shipment>()
            .FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        saved.State.Should().Be(ShipmentState.Pending);
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new MarkShipmentPending.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}