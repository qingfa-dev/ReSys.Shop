using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Payment.Services.Models;
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

    public async Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret))
        {
            return Error.Validation(
                "Stripe.WebhookSecret.NotConfigured",
                "Stripe webhook secret is not configured.");
        }

        // The real handler does its own signature validation against the header.
        // We pass the raw payload and a placeholder signature marker; the gateway
        // pipeline at the endpoint must inject the real Stripe-Signature header
        // before reaching this dispatcher.
        var result = await _sender.Send(new StripeWebhook.Command(payload, "stripe-signature"), ct);
        return result;
    }

    public bool ValidateSignature(string payload, string stripeSignature)
    {
        if (string.IsNullOrEmpty(_options.WebhookSecret)) return false;
        try
        {
            EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe signature validation failed");
            return false;
        }
    }

    public Event? ParseEvent(string payload)
    {
        try { return EventUtility.ParseEvent(payload); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe event parse failed: {Payload}", payload);
            return null;
        }
    }
}
