using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;

using Module.Inventory.Features.Storefront.ConsumeCartStockReservations;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Checkout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class CreateOrderFromCartTransactionTests
{
    [Fact(DisplayName = "CreateOrderFromCart: delegates stock consumption to ISender")]
    public async Task Handle_DelegatesStockConsumption_ToSender()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        var db = new ApplicationDbContext(opts);

        var userId = Guid.NewGuid();

        var cart = new Order
        {
            Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Draft,
            Number = "SEED", Currency = "USD", Email = "u@e.com",
            CheckoutState = CheckoutState.Payment,
            BillAddressId = Guid.NewGuid(), ShipAddressId = Guid.NewGuid(),
            ShippingMethodId = Guid.NewGuid(),
            Total = 0m
        };
        db.Set<Order>().Add(cart);
        db.Set<Module.Ordering.Domain.LineItems.LineItem>().Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = cart.Id, VariantId = Guid.NewGuid(), Quantity = 2
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = new Mock<ILogger<CreateOrderFromCart.CommandHandler>>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(userId.ToString());
        currentUser.Setup(x => x.UserName).Returns("tester");
        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetPaymentForCheckoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentForCheckoutResponse { IsCompleted = true, Amount = 10m });
        sender.Setup(s => s.Send(It.IsAny<MarkPaymentPaidCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        sender.Setup(s => s.Send(It.IsAny<ConsumeCartStockReservationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sut = new CreateOrderFromCart.CommandHandler(
            db, logger.Object, currentUser.Object, notificationService.Object, sender.Object);

        var result = await sut.Handle(
            new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        // Verify ConsumeCartStockReservationsCommand was sent
        sender.Verify(s => s.Send(
            It.IsAny<ConsumeCartStockReservationsCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
