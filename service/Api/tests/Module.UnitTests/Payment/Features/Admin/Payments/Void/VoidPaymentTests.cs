using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Admin.Payments.Void;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Void;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "VoidPayment")]
public class VoidPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly VoidPayment.CommandHandler _handler;

    public VoidPaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.PaymentProfilesSupported).Returns(false);
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Voided"));

        _handler = new VoidPayment.CommandHandler(_dbContext, _gatewayMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should void payment when in Pending state")]
    public async Task Handle_ShouldVoid_WhenPending()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.ResponseCode = "auth-123";
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidPayment.Command(payment.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines void")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Void declined."));

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.ResponseCode = "auth-123";
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidPayment.Command(payment.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in wrong state")]
    public async Task Handle_ShouldFail_WhenCheckout()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidPayment.Command(payment.Id), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when payment does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new VoidPayment.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
