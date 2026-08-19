using Microsoft.Extensions.Logging.Abstractions;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;
using Module.Ordering.Services;
using Module.Shipping.Features.Shared.Commands;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Storefront.CompleteCheckoutForPayment;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CompleteCheckoutForPayment")]
public class CompleteCheckoutForPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CompleteCheckoutForPaymentCommandHandler _handler;

    public CompleteCheckoutForPaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(n => n.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var placementService = new CheckoutPlacementService(
            _dbContext,
            _reservationServiceMock.Object,
            _notificationServiceMock.Object,
            _senderMock.Object,
            NullLogger<CheckoutPlacementService>.Instance);

        _handler = new CompleteCheckoutForPaymentCommandHandler(
            _dbContext,
            _senderMock.Object,
            placementService,
            NullLogger<CompleteCheckoutForPaymentCommandHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Order BuildPlaceableCart()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid(), sessionId: null, shipAddressId: null).Value;
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
        return cart;
    }

    [Fact(DisplayName = "Handle: places the order when payment is completed")]
    public async Task Handle_ShouldPlaceOrder_WhenPaymentCompleted()
    {
        var cart = BuildPlaceableCart();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentForCheckoutResponse>.Ok(new PaymentForCheckoutResponse
            {
                IsCompleted = true,
                Amount = 10m,
                PaymentMethodId = Guid.NewGuid(),
                CompletedAtUtc = DateTimeOffset.UtcNow
            }));

        var result = await _handler.Handle(
            new CompleteCheckoutForPaymentCommand { CartId = cart.Id, PaymentId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Placed.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync([cart.Id], TestContext.Current.CancellationToken);
        persisted!.Status.Should().Be(OrderStatus.Placed);

        _reservationServiceMock.Verify(
            s => s.ConsumeForOrderAsync(cart.Id, It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle: returns Placed=false without consuming when cart is not draft")]
    public async Task Handle_ShouldReturnPlacedFalse_WhenCartNotDraft()
    {
        var cart = BuildPlaceableCart();
        cart.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CompleteCheckoutForPaymentCommand { CartId = cart.Id, PaymentId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Placed.Should().BeFalse();

        _reservationServiceMock.Verify(
            s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handle: recomputes PaymentState to Paid when the capture is completed")]
    public async Task Handle_RecomputesPaymentState_WhenCaptureCompleted()
    {
        var cart = BuildPlaceableCart();
        cart.Total = 100m;
        _dbContext.Set<Order>().Add(cart);

        var capture = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), cart.Id).Value;
        capture.State = PaymentRecordState.Completed;
        capture.CapturedAmount = 100m;
        _dbContext.Set<PaymentCapture>().Add(capture);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentForCheckoutResponse>.Ok(new PaymentForCheckoutResponse
            {
                IsCompleted = true,
                Amount = 100m,
                PaymentMethodId = Guid.NewGuid(),
                CompletedAtUtc = DateTimeOffset.UtcNow
            }));

        var result = await _handler.Handle(
            new CompleteCheckoutForPaymentCommand { CartId = cart.Id, PaymentId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var persisted = await _dbContext.Set<Order>().FindAsync([cart.Id], TestContext.Current.CancellationToken);
        persisted!.PaymentState.Should().Be(OrderPaymentState.Paid);
        persisted.PaymentTotal.Should().Be(100m);
    }

    [Fact(DisplayName = "Handle: returns PaymentNotCompleted when payment is not completed")]
    public async Task Handle_ShouldReturnPaymentNotCompleted_WhenPaymentNotCompleted()
    {
        var cart = BuildPlaceableCart();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentForCheckoutResponse>.Ok(new PaymentForCheckoutResponse
            {
                IsCompleted = false,
                Amount = 10m
            }));

        var result = await _handler.Handle(
            new CompleteCheckoutForPaymentCommand { CartId = cart.Id, PaymentId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Order.PaymentNotCompleted");

        _reservationServiceMock.Verify(
            s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
