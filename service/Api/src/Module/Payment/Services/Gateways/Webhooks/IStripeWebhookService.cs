using Stripe;

namespace Module.Payment.Services.Gateways.Webhooks;

public interface IStripeWebhookService
{
    bool ValidateSignature(string payload, string stripeSignature);
    Event? ParseEvent(string payload);
}
