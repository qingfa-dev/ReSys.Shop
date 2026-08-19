using Module.Billing.Backgrounds;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.WebhookEvents;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;
using Module.Ordering.Features.Storefront.RegressCheckoutState;

using Stripe;
using Stripe.Checkout;

using IStripeWebhookService = Module.Billing.Services.Webhook.IStripeWebhookService;

namespace Module.UnitTests.Payment.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ProcessStripeWebhookEventJob")]
public class ProcessStripeWebhookEventJobReconcileSweepTests : IDisposable
{
    private const int ReconcileBatchSize = 25;

    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly Mock<ILogger<ProcessStripeWebhookEventJob>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IStockReservationService> _stockServiceMock;
    private readonly ProcessStripeWebhookEventJob _job;

    public ProcessStripeWebhookEventJobReconcileSweepTests()
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

    // Runs the job on a checkout.session.expired event with an unknown session id: the
    // handler is a deterministic no-op, so any CompleteCheckoutForPaymentCommand sent must
    // come from the post-route reconciliation sweep.
    private async Task<Guid> RunExpiredUnknownSessionAsync()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event
            {
                Type = "checkout.session.expired",
                Id = Guid.NewGuid().ToString(),
                Data = new EventData
                {
                    Object = new Session { Id = "cs_unknown_sweep" }
                }
            });

        var webhookEvent = new WebhookEvent
        {
            StripeEventId = Guid.NewGuid().ToString(),
            Type = "checkout.session.expired",
            Payload = "{}",
            State = WebhookEventState.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<WebhookEvent>().Add(webhookEvent);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _job.ExecuteAsync(webhookEvent.Id, TestContext.Current.CancellationToken);
        return webhookEvent.Id;
    }

    [Fact(DisplayName = "reconciliation sweep places orders for Completed payments that missed placement")]
    public async Task ReconcileSweep_PlacesOrdersForCompletedPayments()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock.Setup(x => x.Send(
                It.IsAny<CompleteCheckoutForPaymentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CompleteCheckoutForPaymentResponse>.Ok(
                new CompleteCheckoutForPaymentResponse { Placed = true }));

        var eventId = await RunExpiredUnknownSessionAsync();

        _senderMock.Verify(x => x.Send(
            It.Is<CompleteCheckoutForPaymentCommand>(c => c.CartId == orderId && c.PaymentId == payment.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        var processed = await _dbContext.Set<WebhookEvent>().FirstAsync(e => e.Id == eventId);
        processed.State.Should().Be(WebhookEventState.Processed);
    }

    [Fact(DisplayName = "reconciliation sweep is a no-op for an already placed order and still processes the event")]
    public async Task ReconcileSweep_AlreadyPlacedOrder_ProcessesEvent()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var eventId = await RunExpiredUnknownSessionAsync();

        _senderMock.Verify(x => x.Send(
            It.Is<CompleteCheckoutForPaymentCommand>(c => c.CartId == orderId && c.PaymentId == payment.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        var processed = await _dbContext.Set<WebhookEvent>().FirstAsync(e => e.Id == eventId);
        processed.State.Should().Be(WebhookEventState.Processed);
    }

    [Fact(DisplayName = "reconciliation sweep failures never fail the webhook event (best-effort)")]
    public async Task ReconcileSweep_PlacementFailure_DoesNotFailEvent()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock.Setup(x => x.Send(
                It.IsAny<CompleteCheckoutForPaymentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CompleteCheckoutForPaymentResponse>.Failure(
                Error.BadRequest("Ordering.Place.Failed", "placement boom")));

        var eventId = await RunExpiredUnknownSessionAsync();

        var processed = await _dbContext.Set<WebhookEvent>().FirstAsync(e => e.Id == eventId);
        processed.State.Should().Be(WebhookEventState.Processed);
    }

    [Fact(DisplayName = "reconciliation sweep skips payments that are not Completed")]
    public async Task ReconcileSweep_SkipsNonCompletedPayments()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var eventId = await RunExpiredUnknownSessionAsync();

        _senderMock.Verify(x => x.Send(
            It.IsAny<CompleteCheckoutForPaymentCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
        var processed = await _dbContext.Set<WebhookEvent>().FirstAsync(e => e.Id == eventId);
        processed.State.Should().Be(WebhookEventState.Processed);
    }

    [Fact(DisplayName = "reconciliation sweep is bounded to the configured batch size")]
    public async Task ReconcileSweep_IsBoundedToBatchSize()
    {
        for (var i = 0; i < ReconcileBatchSize + 5; i++)
        {
            var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
            payment.State = PaymentRecordState.Completed;
            payment.CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(i);
            _dbContext.Set<PaymentCapture>().Add(payment);
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var eventId = await RunExpiredUnknownSessionAsync();

        _senderMock.Verify(x => x.Send(
            It.IsAny<CompleteCheckoutForPaymentCommand>(),
            It.IsAny<CancellationToken>()), Times.Exactly(ReconcileBatchSize));
        var processed = await _dbContext.Set<WebhookEvent>().FirstAsync(e => e.Id == eventId);
        processed.State.Should().Be(WebhookEventState.Processed);
    }
}