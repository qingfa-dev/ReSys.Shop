using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Services;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderInventory")]
public class OrderInventoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockChecker _stockChecker;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public OrderInventoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(StockItem).Assembly,
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);
        _stockChecker = new StockChecker(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private (Order order, LineItem lineItem) CreateOrderWithLineItem(
        bool isCompleted = false, int lineItemQuantity = 3, int? stockCountOnHand = null)
    {
        var ct = TestContext.Current.CancellationToken;

        var order = new Order
        {
            Number = "R-TEST-001",
            Status = isCompleted ? OrderStatus.Placed : OrderStatus.Draft,
            UserId = Guid.NewGuid(),
            CompletedAtUtc = isCompleted ? DateTimeOffset.UtcNow : null,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        _dbContext.SaveChanges();

        var lineItem = new LineItem
        {
            OrderId = order.Id,
            VariantId = _variantId,
            Quantity = lineItemQuantity,
            Price = 10m,
            Total = 10m * lineItemQuantity
        };
        _dbContext.Set<LineItem>().Add(lineItem);
        _dbContext.SaveChanges();

        order.LineItems.Add(lineItem);

        if (stockCountOnHand.HasValue)
        {
            _dbContext.Set<StockItem>().Add(new StockItem
            {
                VariantId = _variantId,
                StockLocationId = _stockLocationId,
                CountOnHand = stockCountOnHand.Value,
                Backorderable = false
            });
            _dbContext.SaveChanges();
        }

        return (order, lineItem);
    }

    [Fact(DisplayName = "RemoveAsync: Should increment stock by line item quantity")]
    public async Task RemoveAsync_ShouldIncrementStock()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = CreateOrderWithLineItem(stockCountOnHand: 5);
        var inventory = new OrderInventory(order, lineItem, _dbContext, _stockChecker);

        await inventory.RemoveAsync(lineItem.Quantity, ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockItem.CountOnHand.Should().Be(8);

        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(3);
        movement.Reason.Should().Be("returned");
    }

    [Fact(DisplayName = "AddToShipmentAsync: Should decrement stock by quantity")]
    public async Task AddToShipmentAsync_ShouldDecrementStock()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = CreateOrderWithLineItem(stockCountOnHand: 10);
        var inventory = new OrderInventory(order, lineItem, _dbContext, _stockChecker);

        await inventory.AddToShipmentAsync(2, ct);
        await _dbContext.SaveChangesAsync(ct);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockItem.CountOnHand.Should().Be(8);

        var movement = await _dbContext.Set<StockMovement>().FirstOrDefaultAsync(cancellationToken: ct);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(-2);
        movement.Reason.Should().Be("sold");
    }

    [Fact(DisplayName = "VerifyAsync: Should not modify stock when CompletedAtUtc is null")]
    public async Task VerifyAsync_ShouldNotModifyStock_WhenNotCompleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = CreateOrderWithLineItem(stockCountOnHand: 10);
        var inventory = new OrderInventory(order, lineItem, _dbContext, _stockChecker);

        await inventory.VerifyAsync(ct);

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "RemoveAsync: Should not crash when no stock location exists")]
    public async Task RemoveAsync_ShouldNotCrash_WhenNoStockLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = CreateOrderWithLineItem();
        var inventory = new OrderInventory(order, lineItem, _dbContext, _stockChecker);

        await inventory.RemoveAsync(lineItem.Quantity, ct);

        var movements = await _dbContext.Set<StockMovement>().ToListAsync(ct);
        movements.Should().BeEmpty();
    }

    [Fact(DisplayName = "AddToShipmentAsync: Should not crash when no stock location exists")]
    public async Task AddToShipmentAsync_ShouldNotCrash_WhenNoStockLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = CreateOrderWithLineItem();
        var inventory = new OrderInventory(order, lineItem, _dbContext, _stockChecker);

        await inventory.AddToShipmentAsync(2, ct);

        var movements = await _dbContext.Set<StockMovement>().ToListAsync(ct);
        movements.Should().BeEmpty();
    }
}
