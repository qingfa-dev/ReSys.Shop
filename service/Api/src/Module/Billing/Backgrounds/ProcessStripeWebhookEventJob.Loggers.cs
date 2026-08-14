namespace Module.Billing.Backgrounds;

/// <summary>Structured log event definitions for Stripe webhook event job processing.</summary>
public static partial class ProcessStripeWebhookEventJobLoggers
{
    [LoggerMessage(
        EventId = 5006,
        Level = LogLevel.Warning,
        Message = "Cannot complete payment {PaymentId} (state={State}): {Message}")]
    public static partial void CannotCompletePayment(ILogger logger, Guid PaymentId, string State, string? Message);

    [LoggerMessage(
        EventId = 5007,
        Level = LogLevel.Warning,
        Message = "Cannot fail payment {PaymentId} (state={State}): {Message}")]
    public static partial void CannotFailPayment(ILogger logger, Guid PaymentId, string State, string? Message);

    [LoggerMessage(
        EventId = 5008,
        Level = LogLevel.Warning,
        Message = "Cannot refund payment {PaymentId} (state={State}): {Message}")]
    public static partial void CannotRefundPayment(ILogger logger, Guid PaymentId, string State, string? Message);

    [LoggerMessage(
        EventId = 5009,
        Level = LogLevel.Warning,
        Message = "Cannot dispute payment {PaymentId} (state={State}): {Message}")]
    public static partial void CannotDisputePayment(ILogger logger, Guid PaymentId, string State, string? Message);

    [LoggerMessage(
        EventId = 5010,
        Level = LogLevel.Warning,
        Message = "Cannot void payment {PaymentId} (state={State}): {Message}")]
    public static partial void CannotVoidPayment(ILogger logger, Guid PaymentId, string State, string? Message);

    [LoggerMessage(
        EventId = 5011,
        Level = LogLevel.Warning,
        Message = "Failed to parse Stripe webhook event from payload.")]
    public static partial void ParseFailure(ILogger logger);

    [LoggerMessage(
        EventId = 5012,
        Level = LogLevel.Information,
        Message = "Stripe webhook event ignored (no handler): {EventType}")]
    public static partial void EventIgnored(ILogger logger, string EventType);

    [LoggerMessage(
        EventId = 5013,
        Level = LogLevel.Warning,
        Message = "Cannot place order for payment {PaymentId} after checkout session completed: {Message}")]
    public static partial void CannotPlaceOrder(ILogger logger, Guid PaymentId, string? Message);

    [LoggerMessage(
        EventId = 5014,
        Level = LogLevel.Information,
        Message = "Stripe webhook event routed to handler: {EventType}")]
    public static partial void EventRouted(ILogger logger, string EventType);

    [LoggerMessage(
        EventId = 5015,
        Level = LogLevel.Debug,
        Message = "Checkout session lookup: SessionId={SessionId}, PaymentFound={Found}, PaymentId={PaymentId}")]
    public static partial void SessionLookup(ILogger logger, string SessionId, bool Found, Guid? PaymentId);

    [LoggerMessage(
        EventId = 5016,
        Level = LogLevel.Information,
        Message = "Checkout session completed: PaymentId={PaymentId}, PaymentIntentId={PaymentIntentId}")]
    public static partial void CheckoutSessionCompleted(ILogger logger, Guid PaymentId, string? PaymentIntentId);

    [LoggerMessage(
        EventId = 5017,
        Level = LogLevel.Information,
        Message = "Order placed after checkout session completed: PaymentId={PaymentId}")]
    public static partial void OrderPlaced(ILogger logger, Guid PaymentId);

    [LoggerMessage(
        EventId = 5018,
        Level = LogLevel.Information,
        Message = "Checkout session expired: PaymentId={PaymentId}, SessionId={SessionId}")]
    public static partial void CheckoutSessionExpired(ILogger logger, Guid PaymentId, string SessionId);

    [LoggerMessage(
        EventId = 5019,
        Level = LogLevel.Information,
        Message = "Cart regressed to Delivery after session expiry: CartId={CartId}")]
    public static partial void CartRegressedToDelivery(ILogger logger, Guid CartId);

    [LoggerMessage(
        EventId = 5024,
        Level = LogLevel.Information,
        Message = "Checkout session completed but payment_status is not 'paid' (status={PaymentStatus}) — skipping completion and order placement: SessionId={SessionId}")]
    public static partial void SessionNotPaid(ILogger logger, string SessionId, string? PaymentStatus);

    [LoggerMessage(
        EventId = 5025,
        Level = LogLevel.Debug,
        Message = "Dropped stale out-of-order Stripe event {StripeEventId} ({StripeEventType}) for payment {PaymentId}")]
    public static partial void StaleEventDropped(ILogger logger, string StripeEventId, string StripeEventType, Guid PaymentId);

    [LoggerMessage(
        EventId = 5026,
        Level = LogLevel.Warning,
        Message = "Failed to mirror payment state {PaymentState} onto order for payment {PaymentId}: {Message}")]
    public static partial void PaymentStateNotifyFailed(ILogger logger, Guid PaymentId, string PaymentState, string? Message);
}
