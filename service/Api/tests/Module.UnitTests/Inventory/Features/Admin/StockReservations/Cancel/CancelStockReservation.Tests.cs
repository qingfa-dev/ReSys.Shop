using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Cancel;

namespace Module.UnitTests.Inventory.Features.Admin.StockReservations.Cancel;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CancelStockReservation")]
public class CancelStockReservationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CancelStockReservation.CommandHandler _handler;

    public CancelStockReservationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockReservation).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CancelStockReservation.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockReservation> SeedReservation(ReservationState state)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            Guid.NewGuid(), 3, state, DateTimeOffset.UtcNow.AddMinutes(30),
            stockLocationId: Guid.NewGuid(), orderId: Guid.NewGuid());
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "Handler: Releases a Reserved reservation")]
    public async Task Handle_ReleasesReservedReservation()
    {
        var reservation = await SeedReservation(ReservationState.Reserved);

        var result = await _handler.Handle(new CancelStockReservation.Command(reservation.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(ReservationState.Released);
        var fresh = await _dbContext.Set<StockReservation>().FirstAsync(r => r.Id == reservation.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        fresh.State.Should().Be(ReservationState.Released);
    }

    [Fact(DisplayName = "Handler: Returns not-found when reservation does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new CancelStockReservation.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Releases a terminal-state reservation (no state guard in handler)")]
    public async Task Handle_ReleasesTerminalStateReservation()
    {
        var reservation = await SeedReservation(ReservationState.Fulfilled);

        var result = await _handler.Handle(new CancelStockReservation.Command(reservation.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(ReservationState.Released);
    }
}
