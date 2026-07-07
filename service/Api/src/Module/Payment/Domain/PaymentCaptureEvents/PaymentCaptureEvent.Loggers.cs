using Microsoft.Extensions.Logging;

namespace Module.Payment.Domain.PaymentCaptureEvents;

public static partial class PaymentCaptureEventLoggers
{
    [LoggerMessage(
        EventId = 3201,
        Level = LogLevel.Debug,
        Message = "[PaymentCaptureEvent.Recorded]: {Amount} for Payment {PaymentId} ({Id}) by {ActionBy}")]
    public static partial void Recorded(ILogger logger, decimal Amount, Guid PaymentId, Guid Id, string? ActionBy = "System");
}
