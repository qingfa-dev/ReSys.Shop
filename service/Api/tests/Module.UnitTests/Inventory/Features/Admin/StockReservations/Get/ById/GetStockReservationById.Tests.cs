using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Get.ById;

namespace Module.UnitTests.Inventory.Features.Admin.StockReservations.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockReservationById")]
public class GetStockReservationByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockReservationById.QueryHandler _handler;

    public GetStockReservationByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockReservation).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStockReservationById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockReservation> SeedReservation()
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            Guid.NewGuid(), 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            stockLocationId: Guid.NewGuid(), orderId: Guid.NewGuid(), reason: "order hold");
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "Handle: Returns reservation DTO when found")]
    public async Task Handle_ReturnsDto_WhenFound()
    {
        var reservation = await SeedReservation();

        var result = await _handler.Handle(new GetStockReservationById.Query(reservation.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(reservation.Id);
        result.Value.Quantity.Should().Be(3);
        result.Value.State.Should().Be(ReservationState.Reserved);
    }

    [Fact(DisplayName = "Handle: Returns not-found when reservation does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new GetStockReservationById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
