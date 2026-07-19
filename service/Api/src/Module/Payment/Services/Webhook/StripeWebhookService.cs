using Module.Payment.Services.Models;
using Module.Payment.Services.Provider.Stripe;

using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;

namespace Module.Payment.Services.Webhook;

/// <summary>Legacy Stripe webhook handler — validates signatures and parses events.</summary>
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

    /// <summary>Handles an inbound Stripe webhook event, verifying the webhook secret is configured.</summary>
    /// <param name="eventType">The Stripe event type.</param>
    /// <param name="payload">The raw JSON payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A success result or validation error if webhook secret is not configured.</returns>
    public Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
    {
        // Check: Webhook secret must be configured before processing
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            return Task.FromResult<Result>(Error.Validation(
                "Stripe.WebhookSecret.NotConfigured",
                "Stripe webhook secret is not configured."));

        return Task.FromResult(Result.Ok());
    }

    /// <summary>Validates an HMAC-SHA256 signature against the configured Stripe webhook secret.</summary>
    /// <param name="payload">The raw request body.</param>
    /// <param name="stripeSignature">The Stripe-Signature header value.</param>
    /// <returns>True if the signature is valid; false if secret is unconfigured or validation fails.</returns>
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

    /// <summary>Deserializes a Stripe event JSON payload into a typed Stripe.Event.</summary>
    /// <param name="payload">The raw JSON payload.</param>
    /// <returns>The parsed Stripe Event, or null if the payload is malformed.</returns>
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