using IPaymentGatewayActionProvider = Module.Billing.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Billing.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using PaymentGatewayResponse = Module.Billing.Services.Provider.PaymentGatewayResponse;

using Module.Billing.Services.Provider;
using Module.Billing.Services.Processing;
using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.Payment.CreateIntent;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;

using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

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

    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
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
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus"));

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

        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _processingServiceMock.Setup(x => x.ProcessAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentProcessingResult());
        _handler = new CreatePaymentIntent.CommandHandler(_dbContext, _currentUserMock.Object, _gatewayRegistryMock.Object, _processingServiceMock.Object, _reservationServiceMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create payment intent for an order")]
    public async Task Handle_ShouldCreatePayment_WhenOrderExists()
    {
        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderKey = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Handler: Should default an empty currency to USD when calling the gateway")]
    public async Task Handle_ShouldDefaultEmptyCurrency_ToUsd()
    {
        // Regression: PaymentParameters.Currency defaults to "" (not null), so
        // `command.Currency ?? Usd` passed "" to Stripe -> "Invalid currency:".
        string? capturedCurrency = null;
        _processingServiceMock
            .Setup(x => x.ProcessAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentCapture, IPaymentGatewayActionProvider, GatewayOptions, CancellationToken>(
                (_, _, options, _) => capturedCurrency = options.Currency)
            .ReturnsAsync(new PaymentProcessingResult());

        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderKey = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, Currency = string.Empty }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        capturedCurrency.Should().Be("USD");
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines authorization")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _processingServiceMock.Setup(x => x.ProcessAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Card declined."));

        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderKey = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return client secret when gateway provides one")]
    public async Task Handle_Should_Return_ClientSecret()
    {
        _processingServiceMock.Setup(x => x.ProcessAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentCapture, IPaymentGatewayActionProvider, GatewayOptions, CancellationToken>(
                (p, _, _, _) => p.IntentClientSecret = "pi_secret_test123")
            .ReturnsAsync(new PaymentProcessingResult());

        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderKey = "bogus", Active = true };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "Handler: Should use specific PaymentMethodId when provided")]
    public async Task Handle_ShouldUseSpecificPaymentMethod()
    {
        var order = CreateOrder();
        var pmA = new PaymentMethod { Name = "Provider A", Code = "provider_a", ProviderKey = "stripe" };
        var pmB = new PaymentMethod { Name = "Provider B", Code = "provider_b", ProviderKey = "bogus" };
        _dbContext.Set<PaymentMethod>().Add(pmA);
        _dbContext.Set<PaymentMethod>().Add(pmB);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        SetupCartForCheckout(order.Id, 100.00m);

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = pmB.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _gatewayRegistryMock.Verify(x => x.GetGateway("bogus"), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when order not found")]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var notFoundId = Guid.NewGuid();
        _senderMock.Setup(x => x.Send(
            It.Is<GetCartForCheckoutQuery>(q => q.CartId == notFoundId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCartForCheckoutResponse>.NotFound(
                errors: [OrderResult.Errors.NotFound(notFoundId)]));

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = notFoundId }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: does NOT persist PaymentCapture when gateway call fails")]
    public async Task Handle_GatewayFails_NoPaymentPersisted()
    {
        var order = CreateOrder();
        var paymentMethod = CreatePaymentMethod();
        SetupGatewayThatThrows();
        SetupCartForCheckout(order.Id, 100.00m);

        var handler = CreateHandler();
        var command = new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = paymentMethod.Id });
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        VerifyPaymentNotAddedToStore();
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

    private PaymentMethod CreatePaymentMethod()
    {
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card", ProviderKey = "stripe" };
        _dbContext.Set<PaymentMethod>().Add(pm);
        _dbContext.SaveChanges();
        return pm;
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

    private void SetupGatewayThatThrows()
    {
        _processingServiceMock.Setup(x => x.ProcessAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Card declined."));
    }

    private CreatePaymentIntent.CommandHandler CreateHandler()
        => new(_dbContext, _currentUserMock.Object, _gatewayRegistryMock.Object, _processingServiceMock.Object, _reservationServiceMock.Object, _senderMock.Object);

    private void VerifyPaymentNotAddedToStore()
    {
        _dbContext.Set<PaymentCapture>().Count().Should().Be(0);
    }
}
