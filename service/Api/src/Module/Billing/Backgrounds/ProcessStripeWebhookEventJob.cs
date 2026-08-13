using Hangfire;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Provider;
using Module.Billing.Services.Webhook;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;
using Module.Ordering.Features.Storefront.RegressCheckoutState;

using Stripe;
using Stripe.Checkout;

using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Billing.Backgrounds;

/// <summary>Background job that processes Stripe webhook events asynchronously via Hangfire — parses the event and routes to type-specific handlers.</summary>
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
public sealed partial class ProcessStripeWebhookEventJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IStripeWebhookService _webhookService;
    private readonly ILogger<ProcessStripeWebhookEventJob> _logger;
    private readonly ISender _sender;
    private readonly IStockReservationService _stockReservationService;

    public ProcessStripeWebhookEventJob(
        IApplicationDbContext dbContext,
        IStripeWebhookService webhookService,
        ILogger<ProcessStripeWebhookEventJob> logger,
        ISender sender,
        IStockReservationService stockReservationService)
    {
        _dbContext = dbContext;
        _webhookService = webhookService;
        _logger = logger;
        _sender = sender;
        _stockReservationService = stockReservationService;
    }

    /// <summary>Entry point — parses the Stripe event and routes to type-specific handler.</summary>
    /// <param name="payload">The raw Stripe webhook JSON payload.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExecuteAsync(string payload, CancellationToken ct = default)
    {
        // Parse: Deserialize Stripe event from raw JSON
        var stripeEvent = _webhookService.ParseEvent(payload);
        if (stripeEvent is null)
        {
            ProcessStripeWebhookEventJobLoggers.ParseFailure(_logger);
            return;
        }

        ProcessStripeWebhookEventJobLoggers.EventRouted(_logger, stripeEvent.Type);

        // Route: Dispatch to handler by event type
        switch (stripeEvent.Type)
        {
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded:
                await HandlePaymentIntentSucceeded(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed:
                await HandlePaymentIntentFailed(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.ChargeRefunded:
                await HandleChargeRefunded(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated:
                await HandleChargeDisputeCreated(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentRequiresAction:
                ProcessStripeWebhookEventJobLoggers.EventIgnored(_logger, stripeEvent.Type);
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing:
                ProcessStripeWebhookEventJobLoggers.EventIgnored(_logger, stripeEvent.Type);
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled:
                await HandlePaymentIntentCanceled(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(stripeEvent, ct);
                break;
            case GatewayConstants.WebhookEvents.Stripe.CheckoutSessionExpired:
                await HandleCheckoutSessionExpired(stripeEvent, ct);
                break;
        }
    }

    // Webhook: payment_intent.succeeded — transition to Completed
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        // Load: Payment by gateway response code (PaymentIntent ID)
        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event (idempotency by Stripe event ID)
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        // Check: Skip if already completed (idempotency by state)
        if (payment.State == PaymentRecordState.Completed) return;

        var result = payment.Complete();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
    }

    // Webhook: payment_intent.payment_failed — transition to Failed
    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Failed or PaymentRecordState.Void) return;

        var result = payment.Fail();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotFailPayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
    }

    // Webhook: charge.refunded — increment RefundedAmount
    private async Task HandleChargeRefunded(Event stripeEvent, CancellationToken ct)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Void) return;

        // Compute: Delta between new refund amount and existing — only apply if positive
        if (charge.AmountRefunded > 0)
        {
            var newRefunded = charge.AmountRefunded / (decimal)GatewayConstants.Amounts.CentsMultiplier;
            var delta = newRefunded - payment.RefundedAmount;
            if (delta > 0)
            {
                var result = payment.Refund(delta);
                if (result.IsFailure)
                {
                    ProcessStripeWebhookEventJobLoggers.CannotRefundPayment(_logger, payment.Id, payment.State.ToString(), result.Message);
                    return;
                }
            }
        }
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
    }

    // Webhook: charge.dispute.created — transition to Disputed state
    private async Task HandleChargeDisputeCreated(Event stripeEvent, CancellationToken ct)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute is null || string.IsNullOrEmpty(dispute.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == dispute.PaymentIntentId, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Disputed) return;

        var result = payment.Dispute();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotDisputePayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
        _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Void) return;

        var result = payment.Void();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        // Lookup by session id OR the stored PaymentIntent id: after the first
        // completion pass overwrites ResponseCode with the pi_... id, a Hangfire
        // retry must still find the payment to re-attempt placement.
        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.ResponseCode == session.Id
                     || (session.PaymentIntentId != null && p.ResponseCode == session.PaymentIntentId),
                ct);
        ProcessStripeWebhookEventJobLoggers.SessionLookup(_logger, session.Id, payment is not null, payment?.Id);
        if (payment is null) return;

        // Dedup: skip only if this exact Stripe event was fully processed before.
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;

        // Store the PaymentIntent id so admin refund/void and charge.* webhooks can
        // correlate against it (the cs_... session id is rejected by Stripe operations).
        if (!string.IsNullOrEmpty(session.PaymentIntentId))
        {
            payment.ResponseCode = session.PaymentIntentId;
            ProcessStripeWebhookEventJobLoggers.CheckoutSessionCompleted(_logger, payment.Id, session.PaymentIntentId);
        }

        if (payment.State != PaymentRecordState.Completed)
        {
            var complete = payment.Complete();
            if (complete.IsFailure && payment.State != PaymentRecordState.Completed)
            {
                ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), complete.Message);
                return;
            }
            await SaveWithRollbackAsync(payment, ct);
        }

        // Place the order. Idempotent: a no-longer-draft cart is a no-op on retry.
        var placeResult = await _sender.Send(
            new CompleteCheckoutForPaymentCommand { CartId = payment.OrderId, PaymentId = payment.Id }, ct);

        // Record the event as processed only after placement succeeds, so a Hangfire
        // retry re-attempts placement (and does not re-complete, due to the state guard).
        if (placeResult.IsSuccess)
        {
            payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
            await SaveWithRollbackAsync(payment, ct);
            ProcessStripeWebhookEventJobLoggers.OrderPlaced(_logger, payment.Id);
        }
        else
        {
            ProcessStripeWebhookEventJobLoggers.CannotPlaceOrder(_logger, payment.Id, placeResult.Message);
        }
    }

    private async Task HandleCheckoutSessionExpired(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == session.Id, ct);
        ProcessStripeWebhookEventJobLoggers.SessionLookup(_logger, session.Id, payment is not null, payment?.Id);
        if (payment is null) return;

        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Void or PaymentRecordState.Completed) return;

        var voidResult = payment.Void();
        if (voidResult.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(_logger, payment.Id, payment.State.ToString(), voidResult.Message);
            return;
        }

        ProcessStripeWebhookEventJobLoggers.CheckoutSessionExpired(_logger, payment.Id, session.Id);

        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);

        await _stockReservationService.ReleaseReservationsAsync(orderId: payment.OrderId, ct: ct);

        // Un-stick: regress the cart Payment → Delivery so the customer can re-pick a payment method.
        await _sender.Send(
            new RegressCheckoutStateCommand { CartId = payment.OrderId, TargetState = "Delivery" }, ct);
        ProcessStripeWebhookEventJobLoggers.CartRegressedToDelivery(_logger, payment.OrderId);
    }

    /// <summary>Persists changes. On DB failure, lets exception propagate — Hangfire retries with fresh scoped context.</summary>
    private async Task SaveWithRollbackAsync(PaymentCapture payment, CancellationToken ct)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}