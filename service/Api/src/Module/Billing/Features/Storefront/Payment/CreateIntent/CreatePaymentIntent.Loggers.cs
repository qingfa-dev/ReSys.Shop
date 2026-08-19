namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntentLoggers
{
    [LoggerMessage(
        EventId = 6010,
        Level = LogLevel.Information,
        Message = "Stripe Checkout session created: PaymentId={PaymentId}, SessionId={SessionId}, CheckoutUrl={CheckoutUrl}")]
    public static partial void SessionCreated(ILogger logger, Guid PaymentId, string? SessionId, string? CheckoutUrl);

    [LoggerMessage(
        EventId = 6011,
        Level = LogLevel.Information,
        Message = "COD payment intent created: PaymentId={PaymentId}, State=Pending")]
    public static partial void CodIntentCreated(ILogger logger, Guid PaymentId);

    [LoggerMessage(
        EventId = 6012,
        Level = LogLevel.Information,
        Message = "Retry at Payment: voided {Count} stale capture(s) for OrderId={OrderId}")]
    public static partial void RetryVoidedStale(ILogger logger, int Count, Guid OrderId);
}
