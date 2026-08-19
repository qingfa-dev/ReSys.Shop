using IPaymentGatewayActionProvider = Module.Billing.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Billing.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using PaymentGatewayResponse = Module.Billing.Services.Provider.PaymentGatewayResponse;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Admin.Payments.Refund;



namespace Module.UnitTests.Payment.Features.Admin.Payments.Refund;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "RefundPayment")]
public class RefundPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;

    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
    private readonly Mock<ISender> _senderMock;
    private readonly RefundPayment.CommandHandler _handler;

    public RefundPaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.RefundAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus"));

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));


        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _processingServiceMock.Setup(x => x.RefundAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentProcessingResult());
        _senderMock = new Mock<ISender>();
        _handler = new RefundPayment.CommandHandler(_dbContext, _gatewayRegistryMock.Object, _processingServiceMock.Object, _senderMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should refund payment when in Completed state")]
    public async Task Handle_ShouldRefund_WhenCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Complete();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new RefundPayment.Command(payment.Id, new RefundPayment.Request { Amount = 30m }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefundedAmount.Should().Be(30m);
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines refund")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _processingServiceMock.Setup(x => x.RefundAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Refund declined."));

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Complete();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new RefundPayment.Command(payment.Id, new RefundPayment.Request { Amount = 30m }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in wrong state")]
    public async Task Handle_ShouldFail_WhenPending()
    {
        _processingServiceMock.Setup(x => x.RefundAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState.Pending, PaymentRecordState.Completed));

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new RefundPayment.Command(payment.Id, new RefundPayment.Request { Amount = 30m }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when payment does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new RefundPayment.Command(Guid.NewGuid(), new RefundPayment.Request { Amount = 30m }),
            TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
