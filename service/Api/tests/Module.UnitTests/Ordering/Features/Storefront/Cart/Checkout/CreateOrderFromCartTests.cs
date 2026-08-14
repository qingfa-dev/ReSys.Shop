using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;
using Module.Ordering.Services;

using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.StockReservations;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Module.Billing.Services.Provider;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Checkout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CreateOrderFromCart")]
public class CreateOrderFromCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CheckoutPlacementService>> _loggerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly CreateOrderFromCart.CommandHandler _handler;

    public CreateOrderFromCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _loggerMock = new Mock<ILogger<CheckoutPlacementService>>();
        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _senderMock = new Mock<ISender>();
        SetupDefaultSenderResponses();

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var placementService = new CheckoutPlacementService(
            _dbContext, _reservationServiceMock.Object, _notificationServiceMock.Object, _loggerMock.Object);

        _handler = new CreateOrderFromCart.CommandHandler(_dbContext, _currentUserMock.Object, _senderMock.Object, placementService);
    }

    private void SetupDefaultSenderResponses()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = true, Amount = 10m });
        _senderMock
            .Setup(s => s.Send(It.IsAny<MarkPaymentPaidCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create order from cart successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenCartHasItems()
    {
        // Arrange: Create a draft cart with a line item
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 2,
            Price = 29.99m,
            Total = 59.98m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(cart.Id);
        result.Value.Number.Should().StartWith("R");

        // Verify order is now placed
        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { cart.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact(DisplayName = "Handler: Should return failure when cart is empty")]
    public async Task Handle_ShouldReturnFailure_WhenCartEmpty()
    {
        // Arrange: Create empty draft cart (checkout prerequisites set but no items)
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.EmptyOrderCannotFinalize.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when no cart exists")]
    public async Task Handle_ShouldReturnFailure_WhenNoCart()
    {
        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when checkout state is not Payment")]
    public async Task Handle_ShouldReturnFailure_WhenCheckoutStateNotPayment()
    {
        // Arrange: Create draft cart with Confirm state (not Payment)
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.Confirm;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 2,
            Price = 29.99m,
            Total = 59.98m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Order.CheckoutState.InvalidTransition");
    }

    [Fact(DisplayName = "Handler: Should return failure when payment not completed")]
    public async Task Handle_ShouldReturnFailure_WhenPaymentNotCompleted()
    {
        // Arrange: Create draft cart
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: Payment returns not completed
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = false, Amount = 0m });

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Order.PaymentNotCompleted");
    }

    [Fact(DisplayName = "Handler: Should return failure when stock reservation consumption fails")]
    public async Task Handle_ShouldReturnFailure_WhenReservationConsumptionFails()
    {
        // Arrange: Create draft cart
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: Reservation consumption fails
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(StockReservationResult.Errors.NoActiveReservations));

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StockReservationResult.Errors.NoActiveReservations.Code);
    }

    [Fact(DisplayName = "Handler: COD pending payment places order without MarkPaymentPaid")]
    public async Task Handle_ShouldPlaceOrder_WhenCodPaymentPending()
    {
        // Arrange: Create a draft cart with a line item and a pending COD capture
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);

        var capture = PaymentCaptureMethod.Create(10m, Guid.NewGuid(), cart.Id).Value;
        capture.State = PaymentRecordState.Pending;
        capture.ProviderKey = GatewayConstants.Providers.CashOnDelivery;
        _dbContext.Set<PaymentCapture>().Add(capture);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: Payment reports pending + offline (COD)
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = false, State = "Pending", IsOffline = true, Amount = 10m });

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert: order placed and capture left pending
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(cart.Id);
        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { cart.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
        var captureAfter = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == capture.Id);
        captureAfter.State.Should().Be(PaymentRecordState.Pending);

        // Assert: offline payments must not be marked paid via the gateway
        _senderMock.Verify(
            s => s.Send(It.IsAny<MarkPaymentPaidCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Pending gateway payment is rejected")]
    public async Task Handle_ShouldReject_WhenPaymentPendingAndNotOffline()
    {
        // Arrange: Create draft cart
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.PickPaymentMethod;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: Payment reports pending but not offline (gateway)
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = false, State = "Pending", IsOffline = false, Amount = 10m });

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Order.PaymentNotCompleted");
    }
}
