using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockItems.Restock;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Restock;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "RestockStockItem")]
public class RestockStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RestockStockItem.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public RestockStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new RestockStockItem.CommandHandler(_dbContext, _currentUserMock.Object, Mock.Of<ILogger<RestockStockItem.CommandHandler>>());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(int countOnHand, bool backorderable = false)
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = countOnHand, Backorderable = backorderable
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);
        return stockItem;
    }

    [Fact(DisplayName = "Handler: Should increase CountOnHand")]
    public async Task Handle_ShouldIncreaseCountOnHand()
    {
        var item = await SeedStockItem(10);

        var result = await _handler.Handle(
            new RestockStockItem.Command(item.Id, new RestockStockItem.Request { Quantity = 20 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousCountOnHand.Should().Be(10);
        result.Value.NewCountOnHand.Should().Be(30);
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(20);

        var stockItem = await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == item.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        stockItem.CountOnHand.Should().Be(30);
    }

    [Fact(DisplayName = "Handler: Should return failure when quantity zero")]
    public async Task Handle_ShouldReturnFailure_WhenQuantityZero()
    {
        var item = await SeedStockItem(10);

        var result = await _handler.Handle(
            new RestockStockItem.Command(item.Id, new RestockStockItem.Request { Quantity = 0 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when stock item not found")]
    public async Task Handle_ShouldReturnFailure_WhenStockItemNotFound()
    {
        var result = await _handler.Handle(
            new RestockStockItem.Command(Guid.NewGuid(), new RestockStockItem.Request { Quantity = 10 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should fulfill backorders fully")]
    public async Task Handle_ShouldFulfillBackordersFully()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 0, Backorderable = true
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderA, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10)));
        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 2, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, orderB, createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-5)));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new RestockStockItem.Command(stockItem.Id, new RestockStockItem.Request { Quantity = 5 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(2);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.RemainingQuantity.Should().Be(0);
        result.Value.NewCountOnHand.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should partially fulfill backorders")]
    public async Task Handle_ShouldPartiallyFulfillBackorders()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 0, Backorderable = true
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 10, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new RestockStockItem.Command(stockItem.Id, new RestockStockItem.Request { Quantity = 4 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.PartiallyFulfilled.Should().Be(1);
        result.Value.RemainingQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should not fulfill backorders when not backorderable")]
    public async Task Handle_ShouldNotFulfillBackorders_WhenNotBackorderable()
    {
        var ct = TestContext.Current.CancellationToken;
        var stockItem = new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId,
            CountOnHand = 5, Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
            _variantId, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
            _stockLocationId, _orderId, createdAtUtc: DateTimeOffset.UtcNow));
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new RestockStockItem.Command(stockItem.Id, new RestockStockItem.Request { Quantity = 10 }),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.BackordersFulfilled.Should().Be(0);
        result.Value.PartiallyFulfilled.Should().Be(0);
        result.Value.NewCountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Handler: Should create StockMovement with reference")]
    public async Task Handle_ShouldCreateStockMovement_WithReference()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = await SeedStockItem(5);

        var result = await _handler.Handle(
            new RestockStockItem.Command(item.Id, new RestockStockItem.Request
            {
                Quantity = 10,
                Reference = "PO-001",
                Reason = "Summer restock"
            }),
            ct);

        result.IsSuccess.Should().BeTrue();
        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.OriginatorType.Should().Be("Restock");
        movement.Reason.Should().Be("Summer restock");
        movement.Action.Should().Be("restock");
        movement.Quantity.Should().Be(10);
    }
}
