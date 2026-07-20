using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Shared.Commands;
using IPaymentGatewayActionProvider = Module.Payment.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Payment.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;

using Module.Payment.Services.Provider;
using Module.Payment.Services.Processing;

namespace Module.UnitTests.Payment.Features.Shared.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "VoidOrderPayments")]
public class VoidOrderPaymentsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;
    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
    private readonly VoidOrderPaymentsCommandHandler _handler;

    public VoidOrderPaymentsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.PaymentProfilesSupported).Returns(false);

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _handler = new VoidOrderPaymentsCommandHandler(_dbContext, _gatewayRegistryMock.Object, _processingServiceMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should fail when void transaction fails")]
    public async Task Handle_Should_Fail_When_Void_Fails()
    {
        _processingServiceMock
            .Setup(x => x.VoidTransactionAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Void declined."));

        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ProviderKey = "bogus";
        payment.ResponseCode = "auth-123";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidOrderPaymentsCommand { OrderId = orderId, Reason = "Cancellation" }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should succeed when void succeeds")]
    public async Task Handle_Should_Succeed_When_Void_Succeeds()
    {
        _processingServiceMock
            .Setup(x => x.VoidTransactionAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentProcessingResult());

        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ProviderKey = "bogus";
        payment.ResponseCode = "auth-123";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidOrderPaymentsCommand
        {
            OrderId = orderId,
            Reason = "Cancellation"
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }
}
