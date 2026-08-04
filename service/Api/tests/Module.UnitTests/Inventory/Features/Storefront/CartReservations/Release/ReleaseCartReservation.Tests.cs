using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.CartReservations.Release;

namespace Module.UnitTests.Inventory.Features.Storefront.CartReservations.Release;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ReleaseCartReservation")]
public class ReleaseCartReservationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ReleaseCartReservation.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public ReleaseCartReservationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ReleaseCartReservation.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(int countOnHand)
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = countOnHand, Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);
        return stockItem;
    }

    private async Task<StockReservation> SeedReservation(int quantity, ReservationState state = ReservationState.Reserved)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = new StockReservation
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            Quantity = quantity, State = state, CartToken = "test-cart-token",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    [Fact(DisplayName = "Handler: Should release reservation without restoring CountOnHand")]
    public async Task Handle_ShouldReleaseReservation_WithoutRestoringStock()
    {
        var stockItem = await SeedStockItem(10);
        var reservation = await SeedReservation(3);

        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = reservation.Id, CartToken = "test-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<StockReservation>().FindAsync([reservation.Id], TestContext.Current.CancellationToken);
        updated!.State.Should().Be(ReservationState.Released);
        updated.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        // CountOnHand is a soft hold — Reserve never decrements it, so Release must not restore it.
        var reloaded = await _dbContext.Set<StockItem>().FindAsync([stockItem.Id], TestContext.Current.CancellationToken);
        reloaded!.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Handler: Should return failure when reservation token mismatched")]
    public async Task Handle_ShouldReturnFailure_WhenTokenMismatched()
    {
        var reservation = await SeedReservation(3);

        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = reservation.Id, CartToken = "other-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when reservation not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = Guid.NewGuid(), CartToken = "test-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when reservation already released")]
    public async Task Handle_ShouldReturnFailure_WhenAlreadyReleased()
    {
        var reservation = await SeedReservation(3, ReservationState.Released);

        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = reservation.Id, CartToken = "test-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when reservation expired")]
    public async Task Handle_ShouldReturnFailure_WhenExpired()
    {
        var reservation = await SeedReservation(3, ReservationState.Expired);

        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = reservation.Id, CartToken = "test-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when reservation fulfilled")]
    public async Task Handle_ShouldReturnFailure_WhenFulfilled()
    {
        var reservation = await SeedReservation(3, ReservationState.Fulfilled);

        var result = await _handler.Handle(
            new ReleaseCartReservation.Command(new ReleaseCartReservation.Request { ReservationId = reservation.Id, CartToken = "test-cart-token" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
