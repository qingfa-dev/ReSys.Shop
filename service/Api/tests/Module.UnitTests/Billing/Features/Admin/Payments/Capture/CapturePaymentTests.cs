using IPaymentGatewayActionProvider = Module.Billing.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Billing.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using PaymentGatewayResponse = Module.Billing.Services.Provider.PaymentGatewayResponse;

using Module.Billing.Services.Provider;
using Module.Billing.Services.Processing;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Admin.Payments.Capture;

using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Capture;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "CapturePayment")]
public class CapturePaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;

    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
    private readonly CapturePayment.CommandHandler _handler;

    public CapturePaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus"));

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));


        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _processingServiceMock.Setup(x => x.CaptureAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentProcessingResult());
        _handler = new CapturePayment.CommandHandler(
            _dbContext,
            _gatewayRegistryMock.Object,
            _processingServiceMock.Object,
            new Mock<ISender>().Object,
            new Mock<ILogger<CapturePayment.CommandHandler>>().Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should capture payment when in Processing state")]
    public async Task Handle_ShouldCapture_WhenProcessing()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CapturePayment.Request { Amount = 50m };
        var result = await _handler.Handle(new CapturePayment.Command(payment.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CapturedAmount.Should().Be(50m);
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _processingServiceMock.Setup(x => x.CaptureAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Capture declined."));

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new CapturePayment.Command(payment.Id, new CapturePayment.Request { Amount = 50m }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in wrong state")]
    public async Task Handle_ShouldFail_WhenCheckout()
    {
        _processingServiceMock.Setup(x => x.CaptureAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState.Checkout, PaymentRecordState.Completed));

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new CapturePayment.Command(payment.Id, new CapturePayment.Request { Amount = 50m }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when payment does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new CapturePayment.Command(Guid.NewGuid(), new CapturePayment.Request { Amount = 50m }), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
