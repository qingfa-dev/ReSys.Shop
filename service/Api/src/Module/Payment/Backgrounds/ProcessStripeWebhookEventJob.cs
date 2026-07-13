using Microsoft.Extensions.Logging;

using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Models;
using Module.Payment.Services.Webhook;

using Stripe;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Backgrounds;

// Defer: Background job processes Stripe webhook events asynchronously via Hangfire
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

    // Webhook: Entry point — parse event and route to type-specific handler
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
                HandleChargeDisputeCreated(stripeEvent);
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

        payment.Complete();
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

        payment.Fail();
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
            var newRefunded = charge.AmountRefunded / 100m;
            var delta = newRefunded - payment.RefundedAmount;
            if (delta > 0) payment.Refund(delta);
        }
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    // Webhook: charge.dispute.created — log for manual review
    private void HandleChargeDisputeCreated(Event stripeEvent)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute is null) return;
        _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
    }
}