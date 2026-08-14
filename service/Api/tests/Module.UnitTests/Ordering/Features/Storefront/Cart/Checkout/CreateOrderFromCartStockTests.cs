using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;
using Module.Ordering.Services;
using Module.Shipping.Features.Shared.Commands;

using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.StockReservations;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Checkout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CreateOrderFromCart")]
public class CreateOrderFromCartStockTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CheckoutPlacementService>> _loggerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly CreateOrderFromCart.CommandHandler _handler;

    public CreateOrderFromCartStockTests()
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
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = true, Amount = 10m });
        _senderMock
            .Setup(s => s.Send(It.IsAny<MarkPaymentPaidCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateShipmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var placementService = new CheckoutPlacementService(
            _dbContext, _reservationServiceMock.Object, _notificationServiceMock.Object, _senderMock.Object, _loggerMock.Object);

        _handler = new CreateOrderFromCart.CommandHandler(_dbContext, _currentUserMock.Object, _senderMock.Object, placementService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Stock: Should return failure when reservation consumption reports failure")]
    public async Task Handle_ShouldReturnFailure_WhenReservationConsumptionFails()
    {
        // Arrange: Create a draft cart
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
            Quantity = 5,
            Price = 29.99m,
            Total = 149.95m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Setup: Reservation consumption returns failure (simulates insufficient stock)
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(StockReservationResult.Errors.InsufficientStock));

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StockReservationResult.Errors.InsufficientStock.Code);
    }

    [Fact(DisplayName = "Stock: Should verify stock reservations are consumed with correct cart ID")]
    public async Task Handle_ShouldSendCorrectCartId_ToConsumeReservations()
    {
        // Arrange: Create a draft cart
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

        // Assert: Verify reservations were consumed with the correct cart ID
        _reservationServiceMock.Verify(s => s.ConsumeForOrderAsync(cart.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
