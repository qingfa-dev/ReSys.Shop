using BuildingBlocks.Querying.Models;

using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Get.Paged;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetPagedShipments")]
public class GetPagedShipmentsHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetPagedShipments.PagedQueryHandler>> _loggerMock;
    private readonly GetPagedShipments.PagedQueryHandler _handler;

    public GetPagedShipmentsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetPagedShipments.PagedQueryHandler>>();
        _handler = new GetPagedShipments.PagedQueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return paged results when shipments exist")]
    public async Task Handle_ShouldReturnPagedResults_WhenShipmentsExist()
    {
        var shipments = new[]
        {
            ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value,
            ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value,
            ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value
        };
        _dbContext.Set<Shipment>().AddRange(shipments);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPagedShipments.Query(new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Handler: Should return empty result when no shipments")]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoShipments()
    {
        var result = await _handler.Handle(
            new GetPagedShipments.Query(new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}