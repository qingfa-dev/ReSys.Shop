using Module.Billing.Features.Shared.Commands;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Application.Domain.Orders;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

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
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly CancelOrderHandler.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public CancelOrderStockRestoreTests()
    {
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly
        ];
        _dbContext = CreateContext();

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

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CancelOrderHandler.CommandHandler(
            _dbContext, _reservationServiceMock.Object, _senderMock.Object,
            _loggerMock.Object, _currentUserMock.Object, _notificationServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task<Order> SeedPlacedOrderWithShipment(
        Guid? orderUserId = null, ShipmentStatus shipmentStatus = ShipmentStatus.Pending)
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

        _dbContext.Set<LineItem>().Add(new LineItem
        {
            OrderId = order.Id,
            VariantId = _variantId,
            Quantity = 3,
            Price = 10m,
            Total = 30m
        });
        await _dbContext.SaveChangesAsync(ct);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        shipment.Status = shipmentStatus;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);
        await _dbContext.SaveChangesAsync(ct);

        return order;
    }

    [Fact(DisplayName = "Handler: Should cancel shipments, set ShipmentState, and return stock when canceling a placed order")]
    public async Task Handle_ShouldCancelShipments_SetShipmentState_AndReturnStock_WhenCancelPlacedOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment();

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsSuccess.Should().BeTrue();

        var shipment = await _dbContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Canceled);

        var reloadedOrder = await _dbContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        reloadedOrder.ShipmentState.Should().Be(ShipmentState.Canceled);

        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(order.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handler: Should not return stock when order is not placed")]
    public async Task Handle_ShouldNotReturnStock_WhenOrderNotPlaced()
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

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderResult.Errors.InvalidStatusTransition);
        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure and not dispatch void when canceling a non-Placed order")]
    public async Task Handle_ShouldReturnFailure_AndNotDispatchVoid_WhenOrderCompleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = new Order
        {
            Number = "R-TEST-004",
            Status = OrderStatus.Completed,
            UserId = _userId,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderResult.Errors.InvalidStatusTransition);
        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure and not persist cancellation when a shipment cannot be canceled")]
    public async Task Handle_ShouldReturnFailure_AndNotPersist_WhenShipmentCannotBeCanceled()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment(shipmentStatus: ShipmentStatus.Shipped);

        var result = await _handler.Handle(new CancelOrderHandler.Command(order.Id), ct);

        result.IsFailure.Should().BeTrue();

        using var freshContext = CreateContext();
        var persistedOrder = await freshContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persistedOrder.Status.Should().Be(OrderStatus.Placed);

        var shipment = await freshContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Shipped);

        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
