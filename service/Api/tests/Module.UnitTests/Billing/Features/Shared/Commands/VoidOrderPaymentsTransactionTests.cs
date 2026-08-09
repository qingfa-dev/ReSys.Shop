using System.Data;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Shared.Commands;
using Module.Billing.Services.Provider;
using Module.Billing.Services.Processing;
using Shared.Operational.Persistence.Transactions;
using IPaymentGatewayActionProvider = Module.Billing.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Billing.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;

namespace Module.UnitTests.Payment.Features.Shared.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "VoidOrderPayments")]
public class VoidOrderPaymentsTransactionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IDatabaseTransaction> _transactionMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;
    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
    private readonly VoidOrderPaymentsCommandHandler _handler;

    public VoidOrderPaymentsTransactionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _transactionMock = new Mock<IDatabaseTransaction>();

        _dbContextMock = new Mock<IApplicationDbContext>();
        _dbContextMock.Setup(x => x.Set<PaymentCapture>())
            .Returns(_dbContext.Set<PaymentCapture>());
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.PaymentProfilesSupported).Returns(false);

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _handler = new VoidOrderPaymentsCommandHandler(_dbContextMock.Object, _gatewayRegistryMock.Object, _processingServiceMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Should fail when gateway is not registered")]
    public async Task Handle_ShouldFail_When_GatewayNotRegistered()
    {
        _gatewayRegistryMock
            .Setup(x => x.GetGateway("unknown"))
            .Returns(Result<IPaymentGatewayActionProvider>.Failure(
                Error.NotFound("Gateway.Provider.unknown.NotFound", "No gateway")));

        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ProviderKey = "unknown";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidOrderPaymentsCommand { OrderId = orderId, Reason = "Cancellation" }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should rollback when void transaction fails")]
    public async Task Handle_ShouldRollback_When_VoidFails()
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
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
