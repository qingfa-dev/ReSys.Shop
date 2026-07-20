namespace Module.Payment.Backgrounds;

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
}
