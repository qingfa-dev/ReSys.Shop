using Microsoft.Extensions.Logging.Abstractions;

using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Services;
using Module.Shipping.Features.Shared.Commands;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CheckoutPlacementService")]
public class CheckoutPlacementServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ISender> _senderMock;
    private readonly CheckoutPlacementService _service;

    public CheckoutPlacementServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(n => n.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateShipmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _service = new CheckoutPlacementService(
            _dbContext,
            _reservationServiceMock.Object,
            _notificationServiceMock.Object,
            _senderMock.Object,
            NullLogger<CheckoutPlacementService>.Instance);
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
        cart.PaymentMethodId = Guid.NewGuid();
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
        return cart;
    }

    [Fact(DisplayName = "PlaceAsync: consumes stock, places order, notifies, and creates shipment")]
    public async Task PlaceAsync_ShouldPlaceOrder_ConsumeStock_Notify_AndCreateShipment()
    {
        var cart = BuildPlaceableCart();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _service.PlaceAsync(cart, "System", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync([cart.Id], TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
        persisted.Number.Should().StartWith("R");

        _reservationServiceMock.Verify(
            s => s.ConsumeForOrderAsync(
                cart.Id,
                It.Is<IReadOnlyCollection<StockConsumeLine>>(l => l.Count == 1 && l.First().VariantId == cart.LineItems.First().VariantId && l.First().Quantity == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(n => n.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(s => s.Send(
            It.Is<CreateShipmentCommand>(c => c.OrderId == cart.Id && c.ShippingMethodId == cart.ShippingMethodId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PlaceAsync: rejects placement when shipping method is missing")]
    public async Task PlaceAsync_ShouldReject_WhenNoShippingMethod()
    {
        var cart = BuildPlaceableCart();
        cart.ShippingMethodId = null;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _service.PlaceAsync(cart, "System", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Order.DeliveryMethodRequired");

        var persisted = await _dbContext.Set<Order>().FindAsync([cart.Id], TestContext.Current.CancellationToken);
        persisted!.Status.Should().Be(OrderStatus.Draft);

        _senderMock.Verify(s => s.Send(It.IsAny<CreateShipmentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PlaceAsync: does not place order when stock consumption fails")]
    public async Task PlaceAsync_ShouldNotPlace_WhenConsumeFails()
    {
        _reservationServiceMock
            .Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(StockReservationResult.Errors.InsufficientStock));

        var cart = BuildPlaceableCart();
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _service.PlaceAsync(cart, "System", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync([cart.Id], TestContext.Current.CancellationToken);
        persisted!.Status.Should().Be(OrderStatus.Draft);

        _senderMock.Verify(s => s.Send(It.IsAny<CreateShipmentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
