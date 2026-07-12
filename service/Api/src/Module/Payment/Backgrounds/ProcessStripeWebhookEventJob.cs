using Microsoft.Extensions.Logging;

using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Models;
using Module.Payment.Services.Webhook;

using Stripe;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Backgrounds;

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

    public async Task ExecuteAsync(string payload, CancellationToken ct = default)
    {
        var stripeEvent = _webhookService.ParseEvent(payload);
        if (stripeEvent is null)
            return;

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

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent is null) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
        if (payment is null) return;
        if (payment.State == PaymentRecordState.Completed) return;

        payment.Complete();
        await _dbContext.SaveChangesAsync(ct);
    }

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

    private async Task HandleChargeRefunded(Event stripeEvent, CancellationToken ct)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

        var payment = await _dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, ct);
        if (payment is null) return;

        if (charge.AmountRefunded > 0)
        {
            var newRefunded = charge.AmountRefunded / 100m;
            var delta = newRefunded - payment.RefundedAmount;
            if (delta > 0) payment.Refund(delta);
        }
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    private void HandleChargeDisputeCreated(Event stripeEvent)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute is null) return;
        _logger.DisputeCreated(dispute.ChargeId, dispute.Reason ?? "unknown");
    }
}
