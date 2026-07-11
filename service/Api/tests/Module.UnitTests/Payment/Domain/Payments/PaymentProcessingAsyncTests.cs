using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Domain.Payments;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentProcessing")]
public class PaymentProcessingAsyncTests
{
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;

    public PaymentProcessingAsyncTests()
    {
        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.SourceRequired).Returns(false);
    }

    private static PaymentCapture CreatePayment(decimal amount = 100m)
    {
        return PaymentCaptureMethod.Create(amount, Guid.NewGuid(), Guid.NewGuid()).Value;
    }

    private static GatewayOptions CreateGatewayOptions(PaymentCapture payment)
    {
        return new GatewayOptions
        {
            Email = "test@example.com",
            StatementDescriptorSuffix = "Test Order",
            Customer = "test@example.com",
            CustomerId = null,
            Ip = null,
            OrderId = payment.OrderId.ToString(),
            PaymentId = payment.Number,
            IdempotencyKey = $"spree-{payment.Number}",
        };
    }

    #region AuthorizeAsync

    [Fact(DisplayName = "AuthorizeAsync: Should succeed and set state to Pending")]
    public async Task AuthorizeAsync_ShouldSucceed_WhenGatewayAuthorizes()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Authorized", "bogus", authorization: "auth-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.AuthorizeAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
        payment.ResponseCode.Should().Be("auth-xyz");
    }

    [Fact(DisplayName = "AuthorizeAsync: Should fail when gateway declines")]
    public async Task AuthorizeAsync_ShouldFail_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(false, "Card declined", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.AuthorizeAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact(DisplayName = "AuthorizeAsync: Should fail when gateway returns infrastructure error")]
    public async Task AuthorizeAsync_ShouldFail_WhenGatewayInfrastructureError()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected("Gateway.ConnectionError", "Connection timeout"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.AuthorizeAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Gateway.ConnectionError");
    }

    #endregion

    #region PurchaseAsync

    [Fact(DisplayName = "PurchaseAsync: Should succeed and set state to Completed")]
    public async Task PurchaseAsync_ShouldSucceed_WhenGatewayPurchases()
    {
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Purchased", "bogus", authorization: "pur-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.PurchaseAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
        payment.CaptureEventCreated.Should().BeTrue();
    }

    [Fact(DisplayName = "PurchaseAsync: Should fail when gateway declines purchase")]
    public async Task PurchaseAsync_ShouldFail_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(false, "Declined", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.PurchaseAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    #endregion

    #region CaptureAsync

    [Fact(DisplayName = "CaptureAsync: Should capture full amount and complete payment")]
    public async Task CaptureAsync_ShouldSucceed_OnFullCapture()
    {
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Captured", "bogus", authorization: "cap-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Pending;

        var result = await PaymentProcessing.CaptureAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "CaptureAsync: Should capture partial amount")]
    public async Task CaptureAsync_ShouldSucceed_OnPartialCapture()
    {
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Captured", "bogus"));

        var payment = CreatePayment(100m);
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Pending;

        var result = await PaymentProcessing.CaptureAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "CaptureAsync: Should return AlreadyCompleted error when payment is already completed")]
    public async Task CaptureAsync_ShouldReturnAlreadyCompleted_WhenAlreadyCompleted()
    {
        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Completed;

        var result = await PaymentProcessing.CaptureAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be("Payment.AlreadyCompleted");
        _gatewayMock.Verify(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region VoidTransactionAsync

    [Fact(DisplayName = "VoidTransactionAsync: Should void payment via gateway")]
    public async Task VoidTransactionAsync_ShouldSucceed_WhenGatewayVoids()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Voided", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Pending;
        payment.ResponseCode = "auth-123";

        var result = await PaymentProcessing.VoidTransactionAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "VoidTransactionAsync: Should void without gateway when no response code")]
    public async Task VoidTransactionAsync_ShouldSucceed_WhenNoResponseCode()
    {
        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Pending;

        var result = await PaymentProcessing.VoidTransactionAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
        _gatewayMock.Verify(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "VoidTransactionAsync: Should return Ok when already voided")]
    public async Task VoidTransactionAsync_ShouldReturnOk_WhenAlreadyVoided()
    {
        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Void;

        var result = await PaymentProcessing.VoidTransactionAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region CancelAsync

    [Fact(DisplayName = "CancelAsync: Should cancel payment via gateway")]
    public async Task CancelAsync_ShouldSucceed_WhenGatewayCancels()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Canceled", "bogus"));

        var payment = CreatePayment();
        payment.ResponseCode = "auth-123";

        var result = await PaymentProcessing.CancelAsync(payment, _gatewayMock.Object, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "CancelAsync: Should fail when gateway cancel fails")]
    public async Task CancelAsync_ShouldFail_WhenGatewayFails()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected("Gateway.Error", "Cancel error"));

        var payment = CreatePayment();
        payment.ResponseCode = "auth-123";

        var result = await PaymentProcessing.CancelAsync(payment, _gatewayMock.Object, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region CreditAsync

    [Fact(DisplayName = "CreditAsync: Should credit payment via gateway")]
    public async Task CreditAsync_ShouldSucceed_WhenGatewayCredits()
    {
        _gatewayMock.Setup(x => x.RefundAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Credited", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Completed;

        var result = await PaymentProcessing.CreditAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "CreditAsync: Should fail when payment not in completed state")]
    public async Task CreditAsync_ShouldFail_WhenNotCompleted()
    {
        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);
        payment.State = PaymentRecordState.Pending;

        var result = await PaymentProcessing.CreditAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Credit.NotAllowed");
    }

    #endregion

    #region ProcessAsync

    [Fact(DisplayName = "ProcessAsync: Should authorize when AutoCapture is false")]
    public async Task ProcessAsync_ShouldAuthorize_WhenAutoCaptureFalse()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Authorized", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
        _gatewayMock.Verify(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "ProcessAsync: Should purchase when AutoCapture is true")]
    public async Task ProcessAsync_ShouldPurchase_WhenAutoCaptureTrue()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Purchased", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await PaymentProcessing.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
        _gatewayMock.Verify(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CancellationToken Propagation

    [Fact(DisplayName = "CancellationToken: Should propagate to gateway AuthorizeAsync")]
    public async Task CancellationToken_ShouldPropagate_ToGatewayAuthorize()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), token))
            .ReturnsAsync(new PaymentGatewayResponse(true, "Authorized", "bogus"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        await PaymentProcessing.AuthorizeAsync(payment, _gatewayMock.Object, options, token);

        _gatewayMock.Verify(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), token), Times.Once);
    }

    #endregion
}
