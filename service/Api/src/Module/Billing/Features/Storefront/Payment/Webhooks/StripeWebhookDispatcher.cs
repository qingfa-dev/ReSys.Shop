using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Module.Billing.Services.Webhook;

using Stripe;

using StripeSetting = Module.Billing.Services.Provider.Stripe.StripeSetting;

namespace Module.Billing.Features.Storefront.Payment.Webhooks;

/// <summary>
/// Implements <see cref="IStripeWebhookService"/> — validates the HMAC-SHA256
/// signature of inbound webhooks and parses the raw payload. Event routing to
/// state transitions happens in <see cref="Module.Billing.Backgrounds.ProcessStripeWebhookEventJob"/>.
/// </summary>
public sealed class StripeWebhookDispatcher : IStripeWebhookService
{
    private readonly StripeSetting _options;
    private readonly ILogger<StripeWebhookDispatcher> _logger;
    private readonly IHostEnvironment _environment;

    public StripeWebhookDispatcher(
        IOptions<StripeSetting> options,
        ILogger<StripeWebhookDispatcher> logger,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _logger = logger;
        _environment = environment;
    }

    // Webhook: Validate HMAC-SHA256 signature against Stripe webhook secret
    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
        {
            if (_environment.IsDevelopment())
            {
                StripeWebhookDispatcherLoggers.SignatureBypassedInDevelopment(_logger);
                return true;
            }
            StripeWebhookDispatcherLoggers.WebhookSecretMissing(_logger);
            return false;
        }
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            StripeWebhookDispatcherLoggers.SignatureVerified(_logger);
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
        // Catch: StripeException → log and return null (malformed payload)
        try
        {
            Event? parsed = EventUtility.ParseEvent(payload);
            if (parsed is not null)
            {
                StripeWebhookDispatcherLoggers.WebhookEventReceived(_logger, parsed.Type);
            }
            return parsed;
        }
        catch (StripeException ex)
        {
            StripeWebhookDispatcherLoggers.EventParseFailed(_logger, ex, ex.Message, payload.Length);
            return null;
        }
    }
}
