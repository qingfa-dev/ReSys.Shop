using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Module.Payment.Services.Provider;
using Module.Payment.Services.Webhook;

using Stripe;

using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

/// <summary>
/// Implements <see cref="IStripeWebhookService"/> as a thin adapter that
/// delegates event handling to the real <see cref="StripeWebhook.CommandHandler"/> via MediatR.
/// </summary>
public sealed class StripeWebhookDispatcher : IStripeWebhookService
{
    private readonly StripeSetting _options;
    private readonly ISender _sender;
    private readonly ILogger<StripeWebhookDispatcher> _logger;

    public string Provider => GatewayConstants.Providers.Stripe;

    public string[] SupportedEventTypes =>
    [
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded,
        GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed,
        GatewayConstants.WebhookEvents.Stripe.ChargeRefunded,
        GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated
    ];

    public StripeWebhookDispatcher(
        IOptions<StripeSetting> options,
        ISender sender,
        ILogger<StripeWebhookDispatcher> logger)
    {
        _options = options.Value;
        _sender = sender;
        _logger = logger;
    }

    // Webhook: Validate HMAC-SHA256 signature against Stripe webhook secret
    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret)) return false;
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            return true;
        }
        // Suppress: StripeException on invalid signature — returns false without throwing
        catch (StripeException ex)
        {
            StripeWebhookDispatcherLoggers.SignatureValidationFailed(_logger, ex);
            return false;
        }
    }

    // Parse: Deserialize Stripe event JSON — returns null if malformed
    public Event? ParseEvent(string payload)
    {
        // Catch: Exception → log and return null (malformed payload)
        try { return EventUtility.ParseEvent(payload); }
        catch (Exception ex)
        {
            StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, payload);
            return null;
        }
    }
}