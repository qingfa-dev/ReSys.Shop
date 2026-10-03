using Stripe;

namespace Module.Billing.Services.Webhook;

/// <summary>Validates Stripe webhook signatures and parses events.</summary>
public interface IStripeWebhookService
{
    /// <summary>Validates an HMAC-SHA256 signature against the Stripe webhook secret.</summary>
    bool ValidateSignature(string payload, string stripeSignature);
    /// <summary>Deserializes a Stripe event JSON payload.</summary>
    Event? ParseEvent(string payload);
}