using Hangfire;

using Microsoft.Extensions.Logging;

using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Provider;
using Module.Payment.Services.Webhook;

using Stripe;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Backgrounds;

/// <summary>Background job that processes Stripe webhook events asynchronously via Hangfire — parses the event and routes to type-specific handlers.</summary>
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
public sealed partial class ProcessStripeWebhookEventJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IStripeWebhookService _webhookService;
    private readonly ILogger<ProcessStripeWebhookEventJob> _logger;

    public ProcessStripeWebhookEventJob(
        IApplicationDbContext dbContext,
        IStripeWebhookService webhookService,
        ILogger<ProcessStripeWebhookEventJob> logger)
    {
        _dbContext = dbContext;
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>Entry point — parses the Stripe event and routes to type-specific handler.</summary>
    /// <param name="payload">The raw Stripe webhook JSON payload.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExecuteAsync(string payload, CancellationToken ct = default)
    {
        // Parse: Deserialize Stripe event from raw JSON
        var stripeEvent = _webhookService.ParseEvent(payload);
        if (stripeEvent is null)
            return;

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
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentProcessing:
                break;
            case GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled:
                await HandlePaymentIntentCanceled(stripeEvent, ct);
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
        // Check: Skip if already completed (idempotency)
        if (payment.State == PaymentRecordState.Completed) return;

        var result = payment.Complete();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    // Webhook: payment_intent.payment_failed — transition to Failed
    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        var result = payment.Fail();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotFailPayment(_logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    // Webhook: charge.refunded — increment RefundedAmount
    private async Task HandleChargeRefunded(Event stripeEvent, CancellationToken ct)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, ct);
        if (payment is null) return;

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
        await _dbContext.SaveChangesAsync(ct);
    }

    // Webhook: charge.dispute.created — transition to Disputed state
    private async Task HandleChargeDisputeCreated(Event stripeEvent, CancellationToken ct)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute is null || string.IsNullOrEmpty(dispute.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == dispute.PaymentIntentId, ct);
        if (payment is null) return;

        var result = payment.Dispute();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotDisputePayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);
        _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;

        var result = payment.Void();
        if (result.IsFailure)
        {
            ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(
                _logger, payment.Id, payment.State.ToString(), result.Message);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}