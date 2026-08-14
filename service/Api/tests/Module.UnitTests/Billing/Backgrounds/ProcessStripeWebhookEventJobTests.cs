using Microsoft.Extensions.Logging;

using Module.Billing.Backgrounds;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Webhook;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;
using Module.Ordering.Features.Storefront.RegressCheckoutState;

using Stripe;
using Stripe.Checkout;

using IStripeWebhookService = Module.Billing.Services.Webhook.IStripeWebhookService;
using PaymentCapture = Module.Billing.Domain.PaymentCaptures.Payment;

namespace Module.UnitTests.Payment.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ProcessStripeWebhookEventJob")]
public class ProcessStripeWebhookEventJobTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly Mock<ILogger<ProcessStripeWebhookEventJob>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _stockServiceMock;
    private readonly ProcessStripeWebhookEventJob _job;

    public ProcessStripeWebhookEventJobTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _webhookMock = new Mock<IStripeWebhookService>();
        _loggerMock = new Mock<ILogger<ProcessStripeWebhookEventJob>>();
        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(
                It.IsAny<CompleteCheckoutForPaymentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CompleteCheckoutForPaymentResponse>.Ok(new CompleteCheckoutForPaymentResponse()));
        _senderMock.Setup(x => x.Send(
                It.IsAny<RegressCheckoutStateCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _stockServiceMock = new Mock<IStockReservationService>();
        _stockServiceMock.Setup(s => s.ReleaseReservationsAsync(
                It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Ok(1));
        _job = new ProcessStripeWebhookEventJob(
            _dbContext, _webhookMock.Object, _loggerMock.Object,
            _senderMock.Object, _stockServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "payment_intent.succeeded transitions payment to Completed")]
    public async Task HandlePaymentIntentSucceeded_ShouldCompletePayment()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_test123";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.succeeded",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_test123" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "payment_intent.payment_failed transitions to Failed")]
    public async Task HandlePaymentIntentFailed_ShouldFailPayment()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_test456";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.payment_failed",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_test456" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact(DisplayName = "charge.refunded updates refund amount")]
    public async Task HandleChargeRefunded_ShouldUpdateRefundAmount()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_refund123";
        payment.RefundedAmount = 0;
        payment.CapturedAmount = payment.Amount;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.refunded",
                Data = new EventData
                {
                    Object = new Charge
                    {
                        PaymentIntentId = "pi_refund123",
                        AmountRefunded = 2000
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.RefundedAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "Idempotency: completed payment is no-op on second execution")]
    public async Task HandlePaymentIntentSucceeded_ShouldBeIdempotent()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_idem";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.succeeded",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_idem" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);
        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "payment_intent.succeeded does not save when Complete returns failure")]
    public async Task HandlePaymentIntentSucceeded_ShouldNotSave_WhenCompleteFails()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_already_done";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.succeeded",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_already_done" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "payment_intent.payment_failed does not save when Fail returns failure")]
    public async Task HandlePaymentIntentFailed_ShouldNotSave_WhenFailFails()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_cant_fail";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.payment_failed",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_cant_fail" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "charge.dispute.created transitions payment to Disputed")]
    public async Task HandleChargeDisputeCreated_ShouldDisputePayment()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_disputed";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.dispute.created",
                Data = new EventData
                {
                    Object = new Dispute
                    {
                        PaymentIntentId = "pi_disputed",
                        ChargeId = "ch_disputed",
                        Reason = "fraudulent"
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "payment_intent.canceled transitions payment to Void")]
    public async Task HandlePaymentIntentCanceled_ShouldVoidPayment()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_canceled";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.canceled",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_canceled" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "HandlePaymentIntentFailed: skips when payment already Failed")]
    public async Task HandlePaymentIntentFailed_AlreadyFailed_Skips()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Failed;
        payment.ResponseCode = "pi_skip_fail";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.payment_failed",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_skip_fail" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact(DisplayName = "HandleChargeRefunded: skips when payment already Voided")]
    public async Task HandleChargeRefunded_AlreadyVoided_Skips()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;
        payment.ResponseCode = "pi_skip_refund";
        payment.RefundedAmount = 0;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.refunded",
                Data = new EventData
                {
                    Object = new Charge
                    {
                        PaymentIntentId = "pi_skip_refund",
                        AmountRefunded = 2000
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Void);
        updated.RefundedAmount.Should().Be(0);
    }

    [Fact(DisplayName = "HandleChargeDisputeCreated: skips when payment already Disputed")]
    public async Task HandleChargeDisputeCreated_AlreadyDisputed_Skips()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Disputed;
        payment.ResponseCode = "pi_skip_dispute";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.dispute.created",
                Data = new EventData
                {
                    Object = new Dispute
                    {
                        PaymentIntentId = "pi_skip_dispute",
                        ChargeId = "ch_skip_dispute",
                        Reason = "fraudulent"
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "HandlePaymentIntentCanceled: skips when payment already Voided")]
    public async Task HandlePaymentIntentCanceled_AlreadyVoided_Skips()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;
        payment.ResponseCode = "pi_skip_void";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.canceled",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_skip_void" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact(DisplayName = "checkout.session.completed completes payment, stores PaymentIntent id and places order")]
    public async Task HandleCheckoutSessionCompleted_ShouldCompletePaymentAndStoreIntentId()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "cs_checkout_123";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.completed",
                Id = "evt_checkout_123",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_checkout_123", PaymentIntentId = "pi_checkout_123", PaymentStatus = "paid" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
        updated.ResponseCode.Should().Be("pi_checkout_123");
        updated.ProcessedStripeEventIds.Should().Contain("evt_checkout_123");

        _senderMock.Verify(x => x.Send(
            It.Is<CompleteCheckoutForPaymentCommand>(c => c.CartId == orderId && c.PaymentId == payment.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "checkout.session.completed retry finds payment by PaymentIntent id after first pass")]
    public async Task HandleCheckoutSessionCompleted_Retry_FindsPaymentByStoredIntentId()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        // First pass already completed the payment and stored the pi_ id, but
        // placement failed so the event id was not recorded.
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_checkout_retry";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.completed",
                Id = "evt_checkout_retry",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_checkout_retry", PaymentIntentId = "pi_checkout_retry", PaymentStatus = "paid" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.ResponseCode.Should().Be("pi_checkout_retry");
        updated.ProcessedStripeEventIds.Should().Contain("evt_checkout_retry");

        _senderMock.Verify(x => x.Send(
            It.Is<CompleteCheckoutForPaymentCommand>(c => c.CartId == orderId && c.PaymentId == payment.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "checkout.session.expired voids payment and releases reservations")]
    public async Task HandleCheckoutSessionExpired_ShouldVoidAndReleaseReservations()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "cs_expired_456";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.expired",
                Id = "evt_expired_456",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_expired_456" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Void);
        updated.ProcessedStripeEventIds.Should().Contain("evt_expired_456");

        _stockServiceMock.Verify(s => s.ReleaseReservationsAsync(
            payment.OrderId, It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(x => x.Send(
            It.Is<RegressCheckoutStateCommand>(c => c.CartId == orderId && c.TargetState == CheckoutState.PickDeliveryMethod),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "checkout.session.completed with payment_status=unpaid does not complete or place order")]
    public async Task HandleCheckoutSessionCompleted_NotPaid_Skips()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "cs_not_paid_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.completed",
                Id = "evt_not_paid_1",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_not_paid_1", PaymentIntentId = "pi_not_paid_1", PaymentStatus = "unpaid" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Processing);
        updated.ResponseCode.Should().Be("cs_not_paid_1");
        updated.ProcessedStripeEventIds.Should().NotContain("evt_not_paid_1");

        _senderMock.Verify(x => x.Send(
            It.IsAny<CompleteCheckoutForPaymentCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "checkout.session.completed throws when order placement fails so Hangfire retries")]
    public async Task HandleCheckoutSessionCompleted_PlacementFailure_Throws()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "cs_place_fail_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock.Setup(x => x.Send(
                It.IsAny<CompleteCheckoutForPaymentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CompleteCheckoutForPaymentResponse>.Failure(Error.BadRequest("Ordering.Place.Failed", "placement boom")));

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.completed",
                Id = "evt_place_fail_1",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_place_fail_1", PaymentIntentId = "pi_place_fail_1", PaymentStatus = "paid" }
                }
            });

        var act = () => _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Event must remain unrecorded so a Hangfire retry re-attempts placement.
        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.ProcessedStripeEventIds.Should().NotContain("evt_place_fail_1");
    }

    [Fact(DisplayName = "checkout.session.expired still releases reservations and regresses cart for a Failed payment")]
    public async Task HandleCheckoutSessionExpired_FailedPayment_ReleasesAndRegresses()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Failed;
        payment.ResponseCode = "cs_expired_failed_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.expired",
                Id = "evt_expired_failed_1",
                Data = new EventData
                {
                    Object = new Session { Id = "cs_expired_failed_1" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Failed);
        updated.ProcessedStripeEventIds.Should().Contain("evt_expired_failed_1");

        _stockServiceMock.Verify(s => s.ReleaseReservationsAsync(
            payment.OrderId, It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(x => x.Send(
            It.Is<RegressCheckoutStateCommand>(c => c.CartId == orderId && c.TargetState == CheckoutState.PickDeliveryMethod),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "charge.refunded reconciles to the Stripe total without double-counting")]
    public async Task HandleChargeRefunded_ReconcilesWithoutDoubleCount()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_reconcile_1";
        payment.RefundedAmount = 10m; // already tracked an earlier $10 refund
        payment.CapturedAmount = payment.Amount;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.refunded",
                Data = new EventData
                {
                    Object = new Charge
                    {
                        PaymentIntentId = "pi_reconcile_1",
                        AmountRefunded = 2000 // Stripe total = $20
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.RefundedAmount.Should().Be(20m);
    }

    private static DateTime T(int hour) =>
        new(2026, 8, 14, hour, 0, 0, DateTimeKind.Utc);

    [Fact(DisplayName = "payment_intent.payment_failed older than the last applied event is dropped as stale")]
    public async Task HandlePaymentIntentFailed_StaleEvent_IsDropped()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = "pi_stale_1";
        payment.LastStripeEventId = "evt_newer_success";
        payment.LastStripeEventCreatedAtUtc = T(12); // a newer event already applied
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.payment_failed",
                Id = "evt_stale_failed_1",
                Created = T(11), // older than the applied event
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_stale_1" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
        updated.ProcessedStripeEventIds.Should().Contain("evt_stale_failed_1");
    }

    [Fact(DisplayName = "payment_intent.succeeded on a voided payment throws so Hangfire retries")]
    public async Task HandlePaymentIntentSucceeded_OnVoidedPayment_Throws()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;
        payment.ResponseCode = "pi_voided_1";
        payment.LastStripeEventId = "evt_voided_1";
        payment.LastStripeEventCreatedAtUtc = T(9);
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.succeeded",
                Id = "evt_succeeded_after_void",
                Created = T(10),
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_voided_1" }
                }
            });

        var act = () => _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "charge.refunded arriving before completion throws so Hangfire retries after the payment completes")]
    public async Task HandleChargeRefunded_BeforeCompleted_Throws()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing; // session.completed not yet applied
        payment.ResponseCode = "pi_refund_race_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.refunded",
                Data = new EventData
                {
                    Object = new Charge
                    {
                        PaymentIntentId = "pi_refund_race_1",
                        AmountRefunded = 2000
                    }
                }
            });

        var act = () => _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "charge.refunded on a Disputed payment still reconciles the refunded total")]
    public async Task HandleChargeRefunded_Disputed_Reconciles()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Disputed;
        payment.ResponseCode = "pi_dispute_refund_1";
        payment.CapturedAmount = payment.Amount;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "charge.refunded",
                Data = new EventData
                {
                    Object = new Charge
                    {
                        PaymentIntentId = "pi_dispute_refund_1",
                        AmountRefunded = 2000
                    }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.RefundedAmount.Should().Be(20m);
        updated.RefundedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "payment_intent.succeeded stamps CompletedAtUtc business timestamp")]
    public async Task HandlePaymentIntentSucceeded_SetsCompletedAtUtc()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_stamp_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.succeeded",
                Id = "evt_stamp_1",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_stamp_1" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Completed);
        updated.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "payment_intent.canceled stamps VoidedAtUtc business timestamp")]
    public async Task HandlePaymentIntentCanceled_SetsVoidedAtUtc()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ResponseCode = "pi_void_stamp_1";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "payment_intent.canceled",
                Id = "evt_void_stamp_1",
                Data = new EventData
                {
                    Object = new PaymentIntent { Id = "pi_void_stamp_1" }
                }
            });

        await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
        updated.State.Should().Be(PaymentRecordState.Void);
        updated.VoidedAtUtc.Should().NotBeNull();
    }
}
