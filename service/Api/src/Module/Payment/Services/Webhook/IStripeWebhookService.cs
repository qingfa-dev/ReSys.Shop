using Stripe;

namespace Module.Payment.Services.Webhook;

public interface IStripeWebhookService
{
    bool ValidateSignature(string payload, string stripeSignature);
    Event? ParseEvent(string payload);
}
