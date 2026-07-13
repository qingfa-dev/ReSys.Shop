using Stripe;

namespace Module.Payment.Services.Webhook;

// Contract: ValidateSignature returns false on invalid HMAC; ParseEvent returns null on parse failure
public interface IStripeWebhookService
{
    bool ValidateSignature(string payload, string stripeSignature);
    Event? ParseEvent(string payload);
}