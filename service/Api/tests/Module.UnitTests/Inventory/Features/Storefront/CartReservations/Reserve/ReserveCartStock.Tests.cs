using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Storefront.CartReservations.Reserve;

namespace Module.UnitTests.Inventory.Features.Storefront.CartReservations.Reserve;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ReserveCartStock")]
public class ReserveCartStockTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ReserveCartStock.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly string _cartToken = "cart-test-123";

    public ReserveCartStockTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ReserveCartStock.CommandHandler(_dbContext);
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

    private async Task<StockReservation> SeedReservation(int quantity, ReservationState state, DateTimeOffset? expiresAtUtc = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var reservation = StockReservationMethod.SeedForTest(
            _variantId, quantity, state, expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderId: Guid.NewGuid(), createdAtUtc: DateTimeOffset.UtcNow);
        _dbContext.Set<StockReservation>().Add(reservation);
        await _dbContext.SaveChangesAsync(ct);
        return reservation;
    }

    private ReserveCartStock.Command CreateCommand(int quantity, int ttlMinutes = 15)
    {
        return new ReserveCartStock.Command(
            new ReserveCartStock.Request
            {
                VariantId = _variantId,
                StockLocationId = _stockLocationId,
                Quantity = quantity,
                TtlMinutes = ttlMinutes
            },
            _cartToken);
    }

    [Fact(DisplayName = "Handler: Should create reservation with CartToken")]
    public async Task Handle_ShouldCreateReservation_WithCartToken()
    {
        await SeedStockItem(10);

        var result = await _handler.Handle(CreateCommand(3), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.Quantity.Should().Be(3);
        result.Value.State.Should().Be("Reserved");
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(15), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "Handler: Should use custom TTL")]
    public async Task Handle_ShouldUseCustomTtl()
    {
        await SeedStockItem(10);

        var result = await _handler.Handle(CreateCommand(1, ttlMinutes: 5), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "Handler: Should return failure when quantity zero")]
    public async Task Handle_ShouldReturnFailure_WhenQuantityZero()
    {
        var result = await _handler.Handle(CreateCommand(0), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when insufficient stock")]
    public async Task Handle_ShouldReturnFailure_WhenInsufficientStock()
    {
        await SeedStockItem(2);
        await SeedReservation(1, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await _handler.Handle(CreateCommand(2), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should account for other active reservations")]
    public async Task Handle_ShouldAccountForOtherActiveReservations()
    {
        await SeedStockItem(10);
        var otherReservation = await SeedReservation(8, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30));
        otherReservation.CartToken = "other-cart";
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(CreateCommand(3), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
