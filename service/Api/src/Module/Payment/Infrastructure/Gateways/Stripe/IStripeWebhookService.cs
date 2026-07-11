using Stripe;

namespace Module.Payment.Infrastructure.Gateways.Stripe;

public interface IStripeWebhookService
{
    bool ValidateSignature(string payload, string stripeSignature);
    Event? ParseEvent(string payload);
}
