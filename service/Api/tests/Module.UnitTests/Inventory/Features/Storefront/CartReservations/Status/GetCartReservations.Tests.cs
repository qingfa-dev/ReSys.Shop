using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.CartReservations.Status;

namespace Module.UnitTests.Inventory.Features.Storefront.CartReservations.Status;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetCartReservations")]
public class GetCartReservationsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCartReservations.PagedQueryHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly string _cartToken = "cart-test-123";

    public GetCartReservationsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCartReservations.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockReservation> SeedReservation(int quantity, string? cartToken = null, DateTimeOffset? expiresAtUtc = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, ReservationState.Reserved, expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderId: Guid.NewGuid(), cartToken: cartToken ?? _cartToken,
            createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "Handler: Should return active reservations with remaining seconds")]
    public async Task Handle_ShouldReturnActiveReservations_WithRemainingSeconds()
    {
        await SeedReservation(2);
        await SeedReservation(3);

        var result = await _handler.Handle(
            new GetCartReservations.Query(_cartToken, new GetCartReservations.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(r => r.RemainingSeconds.Should().BeGreaterThan(0));
        result.Items.Select(r => r.Quantity).Should().BeEquivalentTo([2, 3]);
    }

    [Fact(DisplayName = "Handler: Should return empty when no reservations")]
    public async Task Handle_ShouldReturnEmpty_WhenNoReservations()
    {
        var result = await _handler.Handle(
            new GetCartReservations.Query("nonexistent", new GetCartReservations.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should ignore expired reservations")]
    public async Task Handle_ShouldIgnoreExpiredReservations()
    {
        await SeedReservation(2, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = await _handler.Handle(
            new GetCartReservations.Query(_cartToken, new GetCartReservations.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should page results when params supplied")]
    public async Task Handle_ShouldPageResults_WhenParamsSupplied()
    {
        await SeedReservation(2);
        await SeedReservation(3);

        var result = await _handler.Handle(
            new GetCartReservations.Query(_cartToken, new GetCartReservations.Parameters { PageSize = 1 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Quantity.Should().Be(2);
        result.TotalCount.Should().Be(2);
    }
}
