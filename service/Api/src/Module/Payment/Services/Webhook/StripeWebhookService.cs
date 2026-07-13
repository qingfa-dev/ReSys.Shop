using Module.Payment.Services.Models;
using Module.Payment.Services.Provider.Stripe;

using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;

namespace Module.Payment.Services.Webhook;

// Context: Legacy webhook handler — use StripeWebhookDispatcher (Features) for current logic
public sealed class StripeWebhookHandler : IWebhookHandler, IStripeWebhookService
{
    private readonly StripeSetting _options;
    private readonly ILogger<StripeWebhookHandler> _logger;

    public string Provider => GatewayConstants.Providers.Stripe;

    public string[] SupportedEventTypes =>
    [
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
        GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
        GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
    ];

    public StripeWebhookHandler(IOptions<StripeSetting> options, ILogger<StripeWebhookHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    // Webhook: Handle inbound Stripe event — checks webhook secret is configured
    public Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
    {
        // Check: Webhook secret must be configured before processing
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            return Task.FromResult<Result>(Error.Validation(
                "Stripe.WebhookSecret.NotConfigured",
                "Stripe webhook secret is not configured."));

        return Task.FromResult(Result.Ok());
    }

    // Webhook: Validate HMAC-SHA256 signature against Stripe webhook secret
    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            return false;
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            return true;
        }
        // Suppress: StripeException on invalid signature — returns false without throwing
        catch (StripeException) { return false; }
    }

    // Parse: Deserialize Stripe event JSON payload
    // Catch: Exception → log and return null (malformed payload)
    public Event? ParseEvent(string payload)
    {
        try
        {
            return EventUtility.ParseEvent(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe event parse failed: {Payload}", payload);
            return null;
        }
    }
}