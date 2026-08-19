namespace Module.Billing.Features.Storefront.Payment.Webhooks;

public static partial class StripeWebhookDispatcherLoggers
{
    [LoggerMessage(
        EventId = 5010,
        Level = LogLevel.Warning,
        Message = "Stripe signature validation failed")]
    public static partial void SignatureValidationFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 5011,
        Level = LogLevel.Error,
        Message = "Stripe event parse failed: {ErrorMessage}. Payload length: {PayloadLength}")]
    public static partial void EventParseFailed(ILogger logger, Exception ex, string ErrorMessage, int PayloadLength);

    [LoggerMessage(
        EventId = 5020,
        Level = LogLevel.Warning,
        Message = "Stripe webhook secret is not configured (GatewayProviders:stripe:WebhookSecret); webhooks rejected outside Development.")]
    public static partial void WebhookSecretMissing(ILogger logger);

    [LoggerMessage(
        EventId = 5023,
        Level = LogLevel.Warning,
        Message = "Development: accepting Stripe webhook without signature verification (stripe listen). Set GatewayProviders:stripe:WebhookSecret to verify.")]
    public static partial void SignatureBypassedInDevelopment(ILogger logger);

    [LoggerMessage(
        EventId = 5021,
        Level = LogLevel.Information,
        Message = "Stripe webhook signature verified.")]
    public static partial void SignatureVerified(ILogger logger);

    [LoggerMessage(
        EventId = 5022,
        Level = LogLevel.Information,
        Message = "Stripe webhook event received: {EventType}")]
    public static partial void WebhookEventReceived(ILogger logger, string EventType);
}
