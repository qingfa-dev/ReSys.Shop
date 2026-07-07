using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Get.ById;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetShipmentById")]
public class GetShipmentByIdHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetShipmentById.QueryHandler>> _loggerMock;
    private readonly GetShipmentById.QueryHandler _handler;

    public GetShipmentByIdHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetShipmentById.QueryHandler>>();
        _handler = new GetShipmentById.QueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return shipment when found")]
    public async Task Handle_ShouldReturnShipment_WhenFound()
    {
        var shipment = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        shipment.Number = "SHP-001";
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShipmentById.Query(shipment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(shipment.Id);
        result.Value.Number.Should().Be("SHP-001");
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new GetShipmentById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Shipment.NotFound");
    }
}