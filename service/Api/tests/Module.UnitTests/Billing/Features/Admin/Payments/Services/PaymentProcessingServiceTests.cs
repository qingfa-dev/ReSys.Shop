using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Processing;
using Module.Billing.Services.Provider;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using PaymentGatewayResponse = Module.Billing.Services.Provider.PaymentGatewayResponse;


using PaymentCapture = Module.Billing.Domain.PaymentCaptures.Payment;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentProcessing")]
public class PaymentProcessingServiceTests
{
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly PaymentProcessingService _service;

    public PaymentProcessingServiceTests()
    {
        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.SourceRequired).Returns(false);
        _service = new PaymentProcessingService();
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
            IdempotencyKey = $"shop-{payment.Number}",
        };
    }

    #region AuthorizeAsync

    [Fact(DisplayName = "AuthorizeAsync: Should succeed and set state to Pending")]
    public async Task AuthorizeAsync_ShouldSucceed_WhenGatewayAuthorizes()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "auth-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
        payment.ResponseCode.Should().Be("auth-xyz");
    }

    [Fact(DisplayName = "AuthorizeAsync: Should fail when gateway declines")]
    public async Task AuthorizeAsync_ShouldFail_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentGatewayResponse>.Failure(Error.BadRequest("Bogus.CardDeclined", "Card was declined.")));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Processing);
    }

    [Fact(DisplayName = "AuthorizeAsync: Should fail when source required but not provided")]
    public async Task AuthorizeAsync_ShouldFail_WhenSourceRequired()
    {
        _gatewayMock.Setup(x => x.SourceRequired).Returns(true);
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "auth-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region PurchaseAsync

    [Fact(DisplayName = "PurchaseAsync: Should succeed and set state to Completed")]
    public async Task PurchaseAsync_ShouldSucceed_WhenGatewayPurchases()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "capture-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "PurchaseAsync: Should fail when gateway declines")]
    public async Task PurchaseAsync_ShouldFail_WhenGatewayDeclines()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentGatewayResponse>.Failure(Error.BadRequest("Bogus.CardDeclined", "Card was declined.")));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Processing);
    }

    [Fact(DisplayName = "PurchaseAsync: Should pass raw string source to gateway")]
    public async Task PurchaseAsync_ShouldPassStringSource_WhenSourceIdIsSet()
    {
        object? capturedSource = null;
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .Callback<decimal, object?, GatewayOptions, CancellationToken>((amount, source, options, ct) => capturedSource = source)
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "capture-xyz"));

        var payment = CreatePayment();
        payment.SourceId = "pm_card_visa";
        payment.SourceType = "card";
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        capturedSource.Should().NotBeNull();
        capturedSource.Should().BeOfType<string>();
        capturedSource.Should().Be("pm_card_visa");
    }

    #endregion

    #region CaptureAsync

    [Fact(DisplayName = "CaptureAsync: Should succeed and set state to Completed")]
    public async Task CaptureAsync_ShouldSucceed_WhenGatewayCaptures()
    {
        _gatewayMock.Setup(x => x.CaptureAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "cap-xyz"));

        var payment = CreatePayment();
        payment.Process();
        var options = CreateGatewayOptions(payment);

        var result = await _service.CaptureAsync(payment, _gatewayMock.Object, options, null, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "CaptureAsync: Should capture partial amount")]
    public async Task CaptureAsync_ShouldCapturePartial_WhenAmountProvided()
    {
        _gatewayMock.Setup(x => x.CaptureAsync(50m, It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "cap-50"));

        var payment = CreatePayment(100m);
        payment.Process();
        var options = CreateGatewayOptions(payment);

        var result = await _service.CaptureAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Processing);
        payment.CapturedAmount.Should().Be(50m);
    }

    [Fact(DisplayName = "CaptureAsync: Should fail when payment not in Process or Pending")]
    public async Task CaptureAsync_ShouldFail_WhenCheckout()
    {
        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.CaptureAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region VoidAsync

    [Fact(DisplayName = "VoidAsync: Should succeed and set state to Void")]
    public async Task VoidAsync_ShouldSucceed_WhenGatewayVoids()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "void-xyz"));

        var payment = CreatePayment();
        payment.Process();
        payment.ResponseCode = "pi_123";
        var options = CreateGatewayOptions(payment);

        var result = await _service.VoidAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "VoidAsync: Should succeed without gateway when no response code")]
    public async Task VoidAsync_ShouldSucceed_WithoutGateway_WhenNoResponseCode()
    {
        var payment = CreatePayment();
        payment.Process();
        payment.ResponseCode = null;
        var options = CreateGatewayOptions(payment);

        var result = await _service.VoidAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "VoidAsync: Should succeed when already voided")]
    public async Task VoidAsync_ShouldSucceed_WhenAlreadyVoid()
    {
        var payment = CreatePayment();
        payment.State = PaymentRecordState.Void;
        var options = CreateGatewayOptions(payment);

        var result = await _service.VoidAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    #endregion

    #region RefundAsync

    [Fact(DisplayName = "RefundAsync: Should succeed and track refunded amount")]
    public async Task RefundAsync_ShouldSucceed_WhenGatewayRefunds()
    {
        _gatewayMock.Setup(x => x.RefundAsync(It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "ref-xyz"));

        var payment = CreatePayment(100m);
        payment.Process();
        payment.Complete();
        payment.CapturedAmount = 100m;
        var options = CreateGatewayOptions(payment);

        var first = await _service.RefundAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        first.IsSuccess.Should().BeTrue();
        payment.RefundedAmount.Should().Be(50m);

        var second = await _service.RefundAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        second.IsSuccess.Should().BeTrue();
        payment.RefundedAmount.Should().Be(100m);

        var over = await _service.RefundAsync(payment, _gatewayMock.Object, options, 10m, TestContext.Current.CancellationToken);

        over.IsFailure.Should().BeTrue();
        payment.RefundedAmount.Should().Be(100m);
    }

    [Fact(DisplayName = "RefundAsync: Should fail when payment not completed")]
    public async Task RefundAsync_ShouldFail_WhenPending()
    {
        var payment = CreatePayment(100m);
        payment.Process();
        var options = CreateGatewayOptions(payment);

        var result = await _service.RefundAsync(payment, _gatewayMock.Object, options, 50m, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ProcessAsync (auto-capture branch)

    [Fact(DisplayName = "ProcessAsync: Should purchase when auto-capture is true")]
    public async Task ProcessAsync_ShouldPurchase_WhenAutoCapture()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);
        _gatewayMock.Setup(x => x.PurchaseAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "pur-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "ProcessAsync: Should authorize when auto-capture is false")]
    public async Task ProcessAsync_ShouldAuthorize_WhenNotAutoCapture()
    {
        _gatewayMock.Setup(x => x.AuthorizeAsync(It.IsAny<decimal>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus", authorization: "auth-xyz"));

        var payment = CreatePayment();
        var options = CreateGatewayOptions(payment);

        var result = await _service.ProcessAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
    }

    #endregion

    #region VoidTransactionAsync

    [Fact(DisplayName = "VoidTransactionAsync: Should void with profile support")]
    public async Task VoidTransactionAsync_ShouldUseSource_WhenProfilesSupported()
    {
        _gatewayMock.Setup(x => x.PaymentProfilesSupported).Returns(true);
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus"));

        var payment = CreatePayment();
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_123";
        var options = CreateGatewayOptions(payment);

        var result = await _service.VoidTransactionAsync(payment, _gatewayMock.Object, options, "src_123", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _gatewayMock.Verify(x => x.VoidAsync("pi_123", "src_123", options, TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region ConfirmAsync

    [Fact(DisplayName = "ConfirmAsync: Should complete when auto-capture")]
    public async Task ConfirmAsync_ShouldComplete_WhenAutoCapture()
    {
        _gatewayMock.Setup(x => x.AutoCapture).Returns(true);

        var payment = CreatePayment();
        var result = await _service.ConfirmAsync(payment, _gatewayMock.Object, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "ConfirmAsync: Should pend when not auto-capture")]
    public async Task ConfirmAsync_ShouldPend_WhenNotAutoCapture()
    {
        var payment = CreatePayment();
        var result = await _service.ConfirmAsync(payment, _gatewayMock.Object, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
    }

    #endregion

    #region CancelAsync (via gateway)

    [Fact(DisplayName = "VoidAsync: Should use CancelAsync flow when response code exists")]
    public async Task VoidAsync_ShouldCallGatewayVoid_WhenResponseCodeExists()
    {
        _gatewayMock.Setup(x => x.VoidAsync(It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("bogus"));

        var payment = CreatePayment();
        payment.Process();
        payment.ResponseCode = "pi_123";
        var options = CreateGatewayOptions(payment);

        var result = await _service.VoidAsync(payment, _gatewayMock.Object, options, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _gatewayMock.Verify(x => x.VoidAsync("pi_123", null, options, TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion
}
