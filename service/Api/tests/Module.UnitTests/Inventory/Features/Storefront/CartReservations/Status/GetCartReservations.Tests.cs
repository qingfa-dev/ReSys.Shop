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
    private readonly GetCartReservations.QueryHandler _handler;
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
        _handler = new GetCartReservations.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockReservation> SeedReservation(int quantity, string? cartToken = null, DateTimeOffset? expiresAtUtc = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            OrderId = Guid.NewGuid(), Quantity = quantity, State = ReservationState.Reserved,
            CartToken = cartToken ?? _cartToken,
            ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "Handler: Should return active reservations with remaining seconds")]
    public async Task Handle_ShouldReturnActiveReservations_WithRemainingSeconds()
    {
        await SeedReservation(2);
        await SeedReservation(3);

        var result = await _handler.Handle(new GetCartReservations.Query(_cartToken), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.RemainingSeconds.Should().BeGreaterThan(0));
        result.Value.Select(r => r.Quantity).Should().BeEquivalentTo([2, 3]);
    }

    [Fact(DisplayName = "Handler: Should return empty when no reservations")]
    public async Task Handle_ShouldReturnEmpty_WhenNoReservations()
    {
        var result = await _handler.Handle(new GetCartReservations.Query("nonexistent"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should ignore expired reservations")]
    public async Task Handle_ShouldIgnoreExpiredReservations()
    {
        await SeedReservation(2, expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = await _handler.Handle(new GetCartReservations.Query(_cartToken), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
