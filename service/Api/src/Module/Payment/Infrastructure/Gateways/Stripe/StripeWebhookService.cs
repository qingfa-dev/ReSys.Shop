using Microsoft.Extensions.Options;
using Module.Payment.Infrastructure.Gateways.Stripe;
using Stripe;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

public interface IStripeWebhookService
{
    bool ValidateSignature(string payload, string stripeSignature);
    Event? ParseEvent(string payload);
}

public sealed class StripeWebhookService : IStripeWebhookService
{
    private readonly StripeOptions _options;

    public StripeWebhookService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>Validates the HMAC-SHA256 signature of an incoming Stripe webhook payload.</summary>
    /// <param name="payload">The raw request body as a string.</param>
    /// <param name="stripeSignature">The value of the Stripe-Signature header.</param>
    /// <returns>True if the signature is valid and the webhook secret is configured; otherwise false.</returns>
    public bool ValidateSignature(string payload, string stripeSignature)
    {
        // Guard: No webhook secret configured — skip validation.
        if (string.IsNullOrEmpty(_options.WebhookSecret)) return false;
        // Verify: HMAC signature matches expected value computed from payload and secret.
        try { EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret); return true; }
        catch (StripeException) { return false; }
    }

    /// <summary>Parses a raw JSON payload into a typed Stripe Event object.</summary>
    /// <param name="payload">The raw JSON payload from the webhook request body.</param>
    /// <returns>A Stripe Event object, or null if parsing fails.</returns>
    public Event? ParseEvent(string payload)
    {
        try { return EventUtility.ParseEvent(payload); }
        catch { return null; }
    }
}
