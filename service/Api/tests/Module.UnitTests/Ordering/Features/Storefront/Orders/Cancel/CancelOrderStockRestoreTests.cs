using Microsoft.Extensions.Logging.Abstractions;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Services;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

using CancelOrderHandler = Module.Ordering.Features.Storefront.Orders.Cancel.CancelOrder;

namespace Module.UnitTests.Ordering.Features.Storefront.Orders.Cancel;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CancelOrderStockRestore")]
public class CancelOrderStockRestoreTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CancelOrderHandler.CommandHandler>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CancelOrderHandler.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public CancelOrderStockRestoreTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(StockItem).Assembly,
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("testuser");

        _loggerMock = new Mock<ILogger<CancelOrderHandler.CommandHandler>>();

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(x => x.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CancelOrderHandler.CommandHandler(
            _dbContext, new StockItemService(_dbContext, NullLogger<StockItemService>.Instance), _senderMock.Object,
            _loggerMock.Object, _currentUserMock.Object, _notificationServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Order> SeedCompletedOrderWithStock(
        int lineItemQuantity = 3, int countOnHand = 10, Guid? orderUserId = null)
    {
        var ct = TestContext.Current.CancellationToken;
        var orderUserIdValue = orderUserId ?? _userId;

        var order = new Order
        {
            Number = "R-TEST-001",
            Status = OrderStatus.Placed,
            UserId = orderUserIdValue,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var lineItem = new LineItem
        {
            OrderId = order.Id,
            VariantId = _variantId,
            Quantity = lineItemQuantity,
            Price = 10m,
            Total = 10m * lineItemQuantity
        };
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem
        {
            VariantId = _variantId,
            StockLocationId = _stockLocationId,
            CountOnHand = countOnHand,
            Backorderable = false
        });
        await _dbContext.SaveChangesAsync(ct);

        return order;
    }

    [Fact(DisplayName = "Handler: Should restore stock when canceling completed order")]
    public async Task Handle_ShouldRestoreStock_WhenCancelCompletedOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedCompletedOrderWithStock(lineItemQuantity: 3, countOnHand: 5);

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockItem.CountOnHand.Should().Be(8);
    }

    [Fact(DisplayName = "Handler: Should create StockMovement when restoring stock on cancel")]
    public async Task Handle_ShouldCreateMovement_OnStockRestore()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedCompletedOrderWithStock(lineItemQuantity: 3, countOnHand: 5);

        await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        var movements = await _dbContext.Set<StockMovement>().ToListAsync(ct);
        movements.Should().HaveCount(1);
        movements[0].Quantity.Should().Be(3);
        movements[0].Reason.Should().Be("returned");
        movements[0].OriginatorType.Should().Be("Order");
    }

    [Fact(DisplayName = "Handler: Should not restore stock when order is not completed")]
    public async Task Handle_ShouldNotRestoreStock_WhenOrderNotCompleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = new Order
        {
            Number = "R-TEST-002",
            Status = OrderStatus.Expired,
            UserId = _userId,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var lineItem = new LineItem
        {
            OrderId = order.Id, VariantId = _variantId,
            Quantity = 2, Price = 10m, Total = 20m
        };
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.Set<StockItem>().Add(new StockItem
        {
            VariantId = _variantId, StockLocationId = _stockLocationId, CountOnHand = 10
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId, ct);
        stockItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Handler: Should return failure when order already canceled")]
    public async Task Handle_ShouldReturnFailure_WhenAlreadyCanceled()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = new Order
        {
            Number = "R-TEST-003", Status = OrderStatus.Canceled,
            UserId = _userId, Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when order not found")]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var result = await _handler.Handle(
            new CancelOrderHandler.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
