using IPaymentGatewayActionProvider = Module.Billing.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using PaymentGatewayResponse = Module.Billing.Services.Provider.PaymentGatewayResponse;

using Module.Billing.Services.Provider;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.Payment.CreateIntent;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "CreatePaymentIntent")]
public class CreatePaymentIntentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;

    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly Mock<ISender> _senderMock;
    private readonly CreatePaymentIntent.CommandHandler _handler;

    public CreatePaymentIntentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(PaymentCapture).Assembly,
            typeof(Order).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("stripe", authorization: "cs_test_1", checkoutUrl: "https://checkout.stripe.com/c/pay/cs_test_1"));

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _senderMock = new Mock<ISender>();
        SetupDefaultSenderResponses();

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock.Setup(s => s.ReserveForVariantAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationMethod.Reserve(
                Guid.NewGuid(), 1, Guid.NewGuid(), null, 30, cartToken: "test"));
        _reservationServiceMock.Setup(s => s.ReleaseReservationsAsync(
                It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Ok(1));

        _handler = new CreatePaymentIntent.CommandHandler(
            _dbContext, _currentUserMock.Object, _gatewayRegistryMock.Object,
            _reservationServiceMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: COD method creates a Pending payment with no gateway call")]
    public async Task Handle_CodMethod_CreatesPendingPayment_NoGateway()
    {
        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Cash on Delivery", Code = "cash_on_delivery",
            ProviderKey = GatewayConstants.Providers.CashOnDelivery, Active = true };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request
                { OrderId = order.Id, PaymentMethodId = pm.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Pending.ToString());
        result.Value.ResponseCode.Should().BeNull();
        result.Value.CheckoutUrl.Should().BeNull();
        _gatewayMock.Verify(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        _dbContext.Set<PaymentCapture>().Single().ProviderKey.Should().Be(GatewayConstants.Providers.CashOnDelivery);
    }

    [Fact(DisplayName = "Handler: Stripe method creates a Checkout Session and maps CheckoutUrl")]
    public async Task Handle_StripeMethod_CreatesCheckoutSession()
    {
        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
            ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request
                { OrderId = order.Id, PaymentMethodId = pm.Id, ReturnUrl = "https://store.test/checkout/return" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ResponseCode.Should().Be("cs_test_1");
        result.Value.CheckoutUrl.Should().Be("https://checkout.stripe.com/c/pay/cs_test_1");
    }

    [Fact(DisplayName = "Handler: does NOT persist PaymentCapture when session creation fails")]
    public async Task Handle_SessionFails_NoPaymentPersisted()
    {
        _gatewayMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Stripe.Error", "Session creation failed."));

        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
            ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = pm.Id }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _dbContext.Set<PaymentCapture>().Count().Should().Be(0);
    }

    private Order CreateOrder()
    {
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var order = OrderMethod.Create("USD", userId, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        order.Total = 100.00m;
        _dbContext.Set<Order>().Add(order);
        _dbContext.SaveChanges();
        return order;
    }

    private void SetupCartForCheckout(Guid cartId, decimal total)
    {
        _senderMock.Setup(x => x.Send(
            It.Is<GetCartForCheckoutQuery>(q => q.CartId == cartId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCartForCheckoutResponse>.Ok(new GetCartForCheckoutResponse
            {
                State = "Delivery",
                Total = total,
                Email = "test@example.com",
                LineItems = []
            }));

        _senderMock.Setup(x => x.Send(
            It.Is<AdvanceCheckoutStateCommand>(c => c.CartId == cartId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    private void SetupDefaultSenderResponses()
    {
        _senderMock.Setup(x => x.Send(
            It.IsAny<GetCartForCheckoutQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCartForCheckoutResponse>.Ok(new GetCartForCheckoutResponse
            {
                State = "Delivery",
                Total = 100.00m,
                Email = "test@example.com",
                LineItems = []
            }));

        _senderMock.Setup(x => x.Send(
            It.IsAny<AdvanceCheckoutStateCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }
}
