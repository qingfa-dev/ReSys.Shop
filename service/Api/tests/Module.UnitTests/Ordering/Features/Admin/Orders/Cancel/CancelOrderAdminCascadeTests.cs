using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Shared.Commands;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Application.Domain.Orders;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

using CancelOrderAdminHandler = Module.Ordering.Features.Admin.Orders.Cancel.CancelOrderAdmin;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Cancel;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CancelOrderAdminCascade")]
public class CancelOrderAdminCascadeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<CancelOrderAdminHandler.CommandHandler>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly CancelOrderAdminHandler.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public CancelOrderAdminCascadeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _loggerMock = new Mock<ILogger<CancelOrderAdminHandler.CommandHandler>>();

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(x => x.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CancelOrderAdminHandler.CommandHandler(
            _dbContext,
            _currentUserMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object,
            _senderMock.Object,
            _reservationServiceMock.Object);
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

    private async Task<Order> SeedPlacedOrderWithShipment(ShipmentStatus shipmentStatus = ShipmentStatus.Pending)
    {
        var ct = TestContext.Current.CancellationToken;

        var order = new Order
        {
            Number = "R-TEST-ADMIN-001",
            Status = OrderStatus.Placed,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
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

        var result = await _handler.Handle(
            new CancelOrderAdminHandler.Command(order.Id, new CancelOrderAdminHandler.Request { Reason = "test" }),
            ct);

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
            Number = "R-TEST-ADMIN-002",
            Status = OrderStatus.Expired,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CancelOrderAdminHandler.Command(order.Id, new CancelOrderAdminHandler.Request { Reason = "test" }),
            ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderResult.Errors.InvalidStatusTransition);
        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure and not dispatch void when canceling a Completed order")]
    public async Task Handle_ShouldReturnFailure_AndNotDispatchVoid_WhenCancelCompletedOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = new Order
        {
            Number = "R-TEST-ADMIN-004",
            Status = OrderStatus.Completed,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Email = "test@test.com",
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CancelOrderAdminHandler.Command(order.Id, new CancelOrderAdminHandler.Request { Reason = "test" }),
            ct);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderResult.Errors.InvalidStatusTransition);
        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure and not dispatch void when a shipment cannot be canceled")]
    public async Task Handle_ShouldReturnFailure_AndNotDispatchVoid_WhenShipmentCannotBeCanceled()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment(shipmentStatus: ShipmentStatus.Delivered);

        var result = await _handler.Handle(
            new CancelOrderAdminHandler.Command(order.Id, new CancelOrderAdminHandler.Request { Reason = "test" }),
            ct);

        result.IsFailure.Should().BeTrue();

        using var freshContext = CreateContext();
        var persistedOrder = await freshContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persistedOrder.Status.Should().Be(OrderStatus.Placed);

        var shipment = await freshContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Delivered);

        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _reservationServiceMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Canceled paid order recomputes PaymentState to CreditOwed and keeps PaymentTotal")]
    public async Task Handle_ShouldRecomputePaymentStateToCreditOwed_WhenCancelingPaidOrder()
    {
        var ct = TestContext.Current.CancellationToken;

        var order = new Order
        {
            Number = "R-TEST-ADMIN-003",
            Status = OrderStatus.Placed,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Email = "test@test.com",
            Total = 0m,
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CapturedAmount = 100m;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new CancelOrderAdminHandler.Command(order.Id, new CancelOrderAdminHandler.Request { Reason = "test" }),
            ct);

        result.IsSuccess.Should().BeTrue();

        using var freshContext = CreateContext();
        var persistedOrder = await freshContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persistedOrder.PaymentState.Should().Be(OrderPaymentState.CreditOwed);
        persistedOrder.PaymentTotal.Should().Be(100m);
    }
}
