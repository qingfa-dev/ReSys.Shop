using Moq;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Module.Payment.Features.Admin.Payments.Capture;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Capture;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "CapturePayment")]
public class CapturePaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly CapturePayment.CommandHandler _handler;

    public CapturePaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentDomain).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PaymentGatewayResponse(true, "Captured")));

        _handler = new CapturePayment.CommandHandler(_dbContext, _gatewayMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should capture payment when in Processing state")]
    public async Task Handle_ShouldCapture_WhenProcessing()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        _dbContext.Set<PaymentDomain>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CapturePayment.Request { Amount = 50m };
        var result = await _handler.Handle(new CapturePayment.Command(payment.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CapturedAmount.Should().Be(50m);
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway declines")]
    public async Task Handle_ShouldReturnFailure_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PaymentGatewayResponse>(Failures.BadRequest("Gateway.Declined", "Card was declined.")));

        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        _dbContext.Set<PaymentDomain>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new CapturePayment.Command(payment.Id, new CapturePayment.Request { Amount = 50m }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in wrong state")]
    public async Task Handle_ShouldFail_WhenCheckout()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<PaymentDomain>().Add(payment);
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
