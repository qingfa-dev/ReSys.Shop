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

    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret)) return false;
        try { EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret); return true; }
        catch (StripeException) { return false; }
    }

    public Event? ParseEvent(string payload)
    {
        try { return EventUtility.ParseEvent(payload); }
        catch { return null; }
    }
}
