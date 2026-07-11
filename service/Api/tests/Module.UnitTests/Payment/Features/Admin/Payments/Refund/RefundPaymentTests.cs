using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Admin.Payments.Refund;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Refund;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "RefundPayment")]
public class RefundPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
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
        _gatewayMock.Setup(x => x.CreditAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Refunded"));

        _handler = new RefundPayment.CommandHandler(_dbContext, _gatewayMock.Object);
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
        _gatewayMock.Setup(x => x.CreditAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
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
