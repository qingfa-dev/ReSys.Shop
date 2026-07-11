using Microsoft.Extensions.Options;
using Module.Payment.Domain.Gateways;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Stripe;
using Stripe;

namespace Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Webhooks;

public sealed class StripeWebhookHandler : IWebhookHandler, IStripeWebhookService
{
    private readonly StripeOptions _options;

    public string Provider => GatewayConstants.Providers.Stripe;

    public string[] SupportedEventTypes =>
    [
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
        GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
        GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
    ];

    public StripeWebhookHandler(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            return Task.FromResult<Result>(Error.Validation(
                "Stripe.WebhookSecret.NotConfigured",
                "Stripe webhook secret is not configured."));

        return Task.FromResult(Result.Ok());
    }

    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            return false;
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            return true;
        }
        catch (StripeException) { return false; }
    }

    public Event? ParseEvent(string payload)
    {
        try { return EventUtility.ParseEvent(payload); }
        catch { return null; }
    }
}
