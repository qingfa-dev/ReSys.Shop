using Microsoft.Extensions.Logging;

namespace Module.Ordering.Domain.Orders;

public static partial class PaymentRecordLoggers
{
    [LoggerMessage(
        EventId = 3101,
        Level = LogLevel.Debug,
        Message = "[PaymentRecord.Created]: Amount {Amount}, State {State} by {ActionBy}")]
    public static partial void Created(ILogger logger, decimal Amount, string State, string? ActionBy = "System");
}
