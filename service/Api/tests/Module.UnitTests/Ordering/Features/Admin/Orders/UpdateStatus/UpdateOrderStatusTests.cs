using Module.Billing.Features.Shared.Commands;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.UpdateStatus;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Application.Domain.Orders;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.UpdateStatus;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderStatus")]
public class UpdateOrderStatusTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateOrderStatus.CommandHandler>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _stockReservationMock;
    private readonly UpdateOrderStatus.CommandHandler _handler;
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public UpdateOrderStatusTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _loggerMock = new Mock<ILogger<UpdateOrderStatus.CommandHandler>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(x => x.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _stockReservationMock = new Mock<IStockReservationService>();
        _stockReservationMock
            .Setup(x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new UpdateOrderStatus.CommandHandler(
            _dbContext,
            _loggerMock.Object,
            _currentUserMock.Object,
            _senderMock.Object,
            _stockReservationMock.Object);
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

        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Email = "test@test.com";
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

    [Fact(DisplayName = "Handler: Should place a draft order when transitioning to placed")]
    public async Task Handle_ShouldPlaceOrder_WhenDraftToPlaced()
    {
        // Arrange
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Id = Guid.NewGuid(), Quantity = 1, Price = 10, VariantId = Guid.NewGuid(), OrderId = order.Id });
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        order.PaymentMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        order.CheckoutState = CheckoutState.Confirm;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Placed };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { order.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact(DisplayName = "Handler: Should cancel shipments, void payments, set ShipmentState, and return stock when canceling a placed order")]
    public async Task Handle_ShouldCancelOrder_WhenPlaced()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment();

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var shipment = await _dbContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Canceled);

        var persisted = await _dbContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persisted.Status.Should().Be(OrderStatus.Canceled);
        persisted.ShipmentState.Should().Be(ShipmentState.Canceled);

        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        _stockReservationMock.Verify(
            x => x.ReturnConsumedForOrderAsync(order.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure and not void payments or return stock when a shipment cannot be canceled")]
    public async Task Handle_ShouldReturnFailure_WhenShipmentCannotBeCanceled()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment(shipmentStatus: ShipmentStatus.Delivered);

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            ct);

        // Assert
        result.IsFailure.Should().BeTrue();

        using var freshContext = CreateContext();
        var persisted = await freshContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persisted.Status.Should().Be(OrderStatus.Placed);

        var shipment = await freshContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Delivered);

        _senderMock.Verify(
            x => x.Send(It.Is<VoidOrderPaymentsCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Never);
        _stockReservationMock.Verify(
            x => x.ReturnConsumedForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should still cancel the order and return stock when voiding payments fails")]
    public async Task Handle_ShouldStillCancelOrder_WhenVoidPaymentsFails()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = await SeedPlacedOrderWithShipment();

        _senderMock
            .Setup(x => x.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Billing.ProviderNotRegistered", "provider not registered")));

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().SingleAsync(o => o.Id == order.Id, ct);
        persisted.Status.Should().Be(OrderStatus.Canceled);

        var shipment = await _dbContext.Set<Shipment>().SingleAsync(ct);
        shipment.Status.Should().Be(ShipmentStatus.Canceled);

        _stockReservationMock.Verify(
            x => x.ReturnConsumedForOrderAsync(order.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 3010),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when order not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
