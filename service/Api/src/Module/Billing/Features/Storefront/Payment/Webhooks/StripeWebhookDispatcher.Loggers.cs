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
}
