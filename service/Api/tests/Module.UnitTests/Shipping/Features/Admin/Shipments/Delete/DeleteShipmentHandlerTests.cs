using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Delete;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "DeleteShipment")]
public class DeleteShipmentHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DeleteShipment.CommandHandler>> _loggerMock;
    private readonly DeleteShipment.CommandHandler _handler;

    public DeleteShipmentHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeleteShipment.CommandHandler>>();
        _handler = new DeleteShipment.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete shipment when found")]
    public async Task Handle_ShouldDeleteShipment_WhenFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteShipment.Command(shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var exists = await _dbContext.Set<Shipment>()
            .AnyAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new DeleteShipment.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}