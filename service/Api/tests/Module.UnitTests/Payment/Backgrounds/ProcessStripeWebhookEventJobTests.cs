using Microsoft.Extensions.Logging;

using Module.Payment.Backgrounds;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Webhook;

using Stripe;

using IStripeWebhookService = Module.Payment.Services.Webhook.IStripeWebhookService;
using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ProcessStripeWebhookEventJob")]
public class ProcessStripeWebhookEventJobTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly Mock<ILogger<ProcessStripeWebhookEventJob>> _loggerMock;
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
        _job = new ProcessStripeWebhookEventJob(_dbContext, _webhookMock.Object, _loggerMock.Object);
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
}
