using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Get.Paged;

namespace Module.UnitTests.Inventory.Features.Admin.StockReservations.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetPagedStockReservations")]
public class GetPagedStockReservationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedStockReservations.PagedQueryHandler _handler;

    public GetPagedStockReservationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockReservation).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPagedStockReservations.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Returns paged stock reservations")]
    public async Task Handle_ReturnsPagedReservations()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 4; i++)
        {
            _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
                Guid.NewGuid(), i + 1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30)));
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetPagedStockReservations.Query(new GetPagedStockReservations.Parameters { PageSize = 2 }),
            ct);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
    }

    [Fact(DisplayName = "Handle: Returns all reservations when no paging params")]
    public async Task Handle_ReturnsAll_WhenNoPagingParams()
    {
        var ct = TestContext.Current.CancellationToken;
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            Guid.NewGuid(), 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30)));
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            Guid.NewGuid(), 4, ReservationState.Fulfilled, DateTimeOffset.UtcNow.AddMinutes(30)));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetPagedStockReservations.Query(new GetPagedStockReservations.Parameters()),
            ct);

        result.Items.Should().HaveCount(2);
    }
}
