using Hangfire;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.WebhookEvents;
using Module.Billing.Services.Provider;
using Module.Billing.Services.Webhook;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;
using Module.Ordering.Features.Storefront.RecordOrderPaymentState;
using Module.Ordering.Features.Storefront.RegressCheckoutState;

using Stripe;
using Stripe.Checkout;



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

    /// <summary>Entry point — claims the persisted event, routes to type-specific handlers and marks the outcome.</summary>
    /// <param name="eventId">The persisted <see cref="WebhookEvent"/> id to process.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExecuteAsync(Guid eventId, CancellationToken ct = default)
    {
        // Load: the persisted event by id — null means it was already removed.
        var webhookEvent = await _dbContext.Set<WebhookEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (webhookEvent is null)
            return;

        // Guard: only Processed events are skipped — a Failed/Processing event is re-claimed.
        if (webhookEvent.State == WebhookEventState.Processed)
            return;

        // Claim: mark Processing and increment the attempt counter.
        webhookEvent.State = WebhookEventState.Processing;
        webhookEvent.AttemptCount += 1;
        await _dbContext.SaveChangesAsync(ct);

        // Parse: Deserialize Stripe event from the stored raw JSON payload
        var stripeEvent = _webhookService.ParseEvent(webhookEvent.Payload);
        if (stripeEvent is null)
        {
            ProcessStripeWebhookEventJobLoggers.ParseFailure(_logger);
            webhookEvent.State = WebhookEventState.Failed;
            await _dbContext.SaveChangesAsync(ct);
            return;
        }

        try
        {
            // Route: Dispatch to handler by event type
            await RouteEventAsync(stripeEvent, ct);
        }
        catch (Exception)
        {
            // Mark Failed and rethrow so Hangfire retries (the event is re-claimed on retry).
            webhookEvent.State = WebhookEventState.Failed;
            await _dbContext.SaveChangesAsync(ct);
            throw;
        }

        webhookEvent.State = WebhookEventState.Processed;
        webhookEvent.ProcessedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task RouteEventAsync(Event stripeEvent, CancellationToken ct)
    {
        ProcessStripeWebhookEventJobLoggers.EventRouted(_logger, stripeEvent.Type);

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
            default:
                ProcessStripeWebhookEventJobLoggers.EventIgnored(_logger, stripeEvent.Type);
                break;
        }
    }

    // Webhook: payment_intent.succeeded — transition to Completed
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        // Load: Payment by Stripe PaymentIntent id (stable), ResponseCode as legacy fallback
        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id || p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event (idempotency by Stripe event ID)
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        // Check: Skip if already completed (idempotency by state)
        if (payment.State == PaymentRecordState.Completed) return;

        var result = payment.Complete(atUtc: StripeEventCreatedUtc(stripeEvent));
        if (result.IsFailure)
        {
            // A succeeded event on a payment that cannot complete (e.g. previously
            // voided/failed) is an anomaly — surface it loudly so Hangfire retries
            // and the reconciliation job can resolve it instead of silently losing
            // a successful charge.
            ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            throw new InvalidOperationException(
                $"Cannot complete payment {payment.Id} on {stripeEvent.Type} (state={payment.State}): {result.Message}");
        }

        await RecordStripeEventAsync(payment, stripeEvent, ct);

        // Mirror: payment succeeded → stamp the order's PaymentCompletedAt timeline.
        await TryNotifyOrderPaymentStateAsync(payment, PaymentTimelineState.Completed, ct);
    }

    // Webhook: payment_intent.payment_failed — transition to Failed
    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id || p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        // Guard: drop an out-of-order failure that is older than a newer applied event
        if (await RecordStaleEventAsync(payment, stripeEvent, ct)) return;
        if (payment.State is PaymentRecordState.Failed or PaymentRecordState.Void) return;

        var result = payment.Fail(atUtc: StripeEventCreatedUtc(stripeEvent));
        if (result.IsFailure)
        {
            // Deliberately do not regress: a payment already Completed/Disputed cannot
            // be marked failed (money moved). Acknowledge the event, log the contradiction.
            ProcessStripeWebhookEventJobLoggers.CannotFailPayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            await RecordStripeEventAsync(payment, stripeEvent, ct);
            return;
        }

        await RecordStripeEventAsync(payment, stripeEvent, ct);

        // Mirror: payment failed → stamp the order's PaymentFailedAt timeline.
        await TryNotifyOrderPaymentStateAsync(payment, PaymentTimelineState.Failed, ct);
    }

    // Webhook: charge.refunded — reconcile RefundedAmount with the Stripe total
    private async Task HandleChargeRefunded(Event stripeEvent, CancellationToken ct)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == charge.PaymentIntentId || p.ResponseCode == charge.PaymentIntentId, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Void) return;

        // Reconcile: Stripe reports the TOTAL refunded amount, not a delta. Set the
        // local total to the reported value (monotonic) so an admin refund racing
        // this webhook does not double-count the same money.
        if (charge.AmountRefunded > 0)
        {
            var totalRefunded = charge.AmountRefunded / (decimal)GatewayConstants.Amounts.CentsMultiplier;
            var result = payment.ReconcileRefunded(totalRefunded);
            if (result.IsFailure)
            {
                // Refund arriving before the payment is Completed is retryable — once
                // checkout.session.completed/payment_intent.succeeded lands, this
                // reconciliation succeeds. Throw so Hangfire retries instead of losing it.
                ProcessStripeWebhookEventJobLoggers.CannotRefundPayment(_logger, payment.Id, payment.State.ToString(), result.Message);
                throw new InvalidOperationException(
                    $"Cannot reconcile refund for payment {payment.Id} on {stripeEvent.Type} (state={payment.State}): {result.Message}");
            }
        }
        await RecordStripeEventAsync(payment, stripeEvent, ct);
    }

    // Webhook: charge.dispute.created — transition to Disputed state
    private async Task HandleChargeDisputeCreated(Event stripeEvent, CancellationToken ct)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute is null || string.IsNullOrEmpty(dispute.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == dispute.PaymentIntentId || p.ResponseCode == dispute.PaymentIntentId, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        if (payment.State is PaymentRecordState.Disputed) return;

        var result = payment.Dispute(atUtc: StripeEventCreatedUtc(stripeEvent));
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotDisputePayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            throw new InvalidOperationException(
                $"Cannot dispute payment {payment.Id} on {stripeEvent.Type} (state={payment.State}): {result.Message}");
        }

        await RecordStripeEventAsync(payment, stripeEvent, ct);
        _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id || p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        // Guard: Skip duplicate event
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        // Guard: drop an out-of-order cancel that is older than a newer applied event
        if (await RecordStaleEventAsync(payment, stripeEvent, ct)) return;
        // Skip: already voided, or already failed (a failed PaymentIntent being
        // cleaned up by cancellation is a no-op — no charge to void).
        if (payment.State is PaymentRecordState.Void or PaymentRecordState.Failed) return;

        var result = payment.Void(atUtc: StripeEventCreatedUtc(stripeEvent));
        if (result.IsFailure)
        {
            // Deliberately do not regress an already Completed/Disputed payment.
            ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            await RecordStripeEventAsync(payment, stripeEvent, ct);
            return;
        }

        await RecordStripeEventAsync(payment, stripeEvent, ct);
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        // Lookup by StripeSessionId (primary), StripePaymentIntentId (retry after the
        // first completion pass), or ResponseCode (legacy fallback).
        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.StripeSessionId == session.Id
                     || (session.PaymentIntentId != null && p.StripePaymentIntentId == session.PaymentIntentId)
                     || p.ResponseCode == session.Id
                     || (session.PaymentIntentId != null && p.ResponseCode == session.PaymentIntentId),
                ct);
        ProcessStripeWebhookEventJobLoggers.SessionLookup(_logger, session.Id, payment is not null, payment?.Id);
        if (payment is null) return;

        // Dedup: skip only if this exact Stripe event was fully processed before.
        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;

        // Guard: only treat the session as paid when Stripe reports payment_status=paid.
        // checkout.session.completed also fires for async methods that are still
        // processing (payment_status=unpaid); completing early would falsely mark a
        // not-yet-paid order as paid.
        if (!string.Equals(session.PaymentStatus, GatewayConstants.Stripe.PaymentStatus.Paid, StringComparison.Ordinal))
        {
            ProcessStripeWebhookEventJobLoggers.SessionNotPaid(_logger, session.Id, session.PaymentStatus);
            return;
        }

        // Store the PaymentIntent id so admin refund/void and charge.* webhooks can
        // correlate against it (the cs_... session id is rejected by Stripe operations).
        if (!string.IsNullOrEmpty(session.PaymentIntentId))
        {
            payment.StripePaymentIntentId = session.PaymentIntentId;
            payment.ResponseCode = session.PaymentIntentId;
            ProcessStripeWebhookEventJobLoggers.CheckoutSessionCompleted(_logger, payment.Id, session.PaymentIntentId);
        }

        if (payment.State != PaymentRecordState.Completed)
        {
            var complete = payment.Complete(atUtc: StripeEventCreatedUtc(stripeEvent));
            if (complete.IsFailure && payment.State != PaymentRecordState.Completed)
            {
                // A completed session on a payment that cannot complete (e.g. it was
                // voided by an out-of-order session.expired) is an anomaly — throw so
                // Hangfire retries and the reconciliation job can revive the order.
                ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), complete.Message);
                throw new InvalidOperationException(
                    $"Cannot complete payment {payment.Id} on {stripeEvent.Type} (state={payment.State}): {complete.Message}");
            }
            await SaveAsync(payment, ct);
        }

        // Place the order. Idempotent: a no-longer-draft cart is a no-op on retry.
        var placeResult = await _sender.Send(
            new CompleteCheckoutForPaymentCommand { CartId = payment.OrderId, PaymentId = payment.Id }, ct);

        // Record the event as processed only after placement succeeds, so a Hangfire
        // retry re-attempts placement (and does not re-complete, due to the state guard).
        if (placeResult.IsSuccess)
        {
            await RecordStripeEventAsync(payment, stripeEvent, ct);
            ProcessStripeWebhookEventJobLoggers.OrderPlaced(_logger, payment.Id);
        }
        else
        {
            // Throw so Hangfire retries: the payment is already Completed and the event
            // id is not yet recorded, so the retry only re-attempts order placement.
            ProcessStripeWebhookEventJobLoggers.CannotPlaceOrder(_logger, payment.Id, placeResult.Message);
            throw new InvalidOperationException(
                $"Order placement failed for payment {payment.Id}: {placeResult.Message}");
        }
    }

    private async Task HandleCheckoutSessionExpired(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id || p.ResponseCode == session.Id, ct);
        ProcessStripeWebhookEventJobLoggers.SessionLookup(_logger, session.Id, payment is not null, payment?.Id);
        if (payment is null) return;

        if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
        // Guard: drop an out-of-order expiry that is older than a newer applied event
        if (await RecordStaleEventAsync(payment, stripeEvent, ct)) return;

        // A paid session must never be expired. A Failed capture cannot be voided,
        // but it still needs the compensating side-effects below (release + regress).
        if (payment.State is PaymentRecordState.Completed) return;

        if (payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending)
        {
            var voidResult = payment.Void(atUtc: StripeEventCreatedUtc(stripeEvent));
            if (voidResult.IsFailure)
            {
                ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(_logger, payment.Id, payment.State.ToString(), voidResult.Message);
                throw new InvalidOperationException(
                    $"Cannot void payment {payment.Id} on {stripeEvent.Type} (state={payment.State}): {voidResult.Message}");
            }
            ProcessStripeWebhookEventJobLoggers.CheckoutSessionExpired(_logger, payment.Id, session.Id);
        }

        // Compensate: release reservations and regress the cart BEFORE recording the
        // event, so a failure here leaves the event unrecorded and a Hangfire retry
        // re-runs these idempotent side-effects.
        var releaseResult = await _stockReservationService.ReleaseReservationsAsync(
            cartToken: payment.OrderId.ToString(), ct: ct);
        if (releaseResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to release stock reservations for order {payment.OrderId}: {releaseResult.Message}");

        var regressResult = await _sender.Send(
            new RegressCheckoutStateCommand { CartId = payment.OrderId, TargetState = CheckoutState.PickDeliveryMethod }, ct);
        if (regressResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to regress cart {payment.OrderId} to Delivery: {regressResult.Message}");

        await RecordStripeEventAsync(payment, stripeEvent, ct);
        ProcessStripeWebhookEventJobLoggers.CartRegressedToDelivery(_logger, payment.OrderId);
    }

    /// <summary>
    /// Drops an out-of-order regression event (payment_failed / payment_intent.canceled /
    /// checkout.session.expired) that is OLDER than the last applied event, acknowledging
    /// it as processed so it is not re-processed. Only regression events are guarded — a
    /// newer progress event must never be dropped by this check.
    /// </summary>
    private async Task<bool> RecordStaleEventAsync(PaymentCapture payment, Event stripeEvent, CancellationToken ct)
    {
        if (payment.LastStripeEventCreatedAtUtc is null)
            return false;

        if (StripeEventCreatedUtc(stripeEvent) > payment.LastStripeEventCreatedAtUtc.Value.ToUniversalTime())
            return false;

        ProcessStripeWebhookEventJobLoggers.StaleEventDropped(_logger, stripeEvent.Id, stripeEvent.Type, payment.Id);
        await RecordStripeEventAsync(payment, stripeEvent, ct);
        return true;
    }

    /// <summary>Marks the event as processed and tracks the latest applied Stripe event time.</summary>
    private async Task RecordStripeEventAsync(PaymentCapture payment, Event stripeEvent, CancellationToken ct)
    {
        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        payment.LastStripeEventId = stripeEvent.Id;
        payment.LastStripeEventCreatedAtUtc = StripeEventCreatedUtc(stripeEvent);
        payment.ProcessedAtUtc = DateTimeOffset.UtcNow;
        await SaveAsync(payment, ct);
    }

    // Mirror: best-effort stamp of the owning order's payment timeline. The payment row
    // is authoritative; a failure here must not fail the job (the order placement path
    // re-stamps on completion via CompleteCheckoutForPayment).
    private async Task TryNotifyOrderPaymentStateAsync(PaymentCapture payment, PaymentTimelineState paymentState, CancellationToken ct)
    {
        var atUtc = paymentState switch
        {
            PaymentTimelineState.Completed => payment.CompletedAtUtc,
            PaymentTimelineState.Failed => payment.FailedAtUtc,
            _ => null
        } ?? DateTimeOffset.UtcNow;

        var result = await _sender.Send(new RecordOrderPaymentStateCommand
        {
            OrderId = payment.OrderId,
            PaymentState = paymentState,
            AtUtc = atUtc
        }, ct);

        if (result.IsFailure)
            ProcessStripeWebhookEventJobLoggers.PaymentStateNotifyFailed(_logger, payment.Id, paymentState.ToString(), result.Message);
    }

    // Convert: Stripe Event.Created (Unix epoch seconds) to a UTC instant regardless of Kind.
    private static DateTime StripeEventCreatedUtc(Event stripeEvent) =>
        stripeEvent.Created.Kind switch
        {
            DateTimeKind.Utc => stripeEvent.Created,
            DateTimeKind.Local => stripeEvent.Created.ToUniversalTime(),
            _ => DateTime.SpecifyKind(stripeEvent.Created, DateTimeKind.Utc)
        };

    /// <summary>Persists changes. On DB failure, lets exception propagate — Hangfire retries with fresh scoped context.</summary>
    private async Task SaveAsync(PaymentCapture payment, CancellationToken ct)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
