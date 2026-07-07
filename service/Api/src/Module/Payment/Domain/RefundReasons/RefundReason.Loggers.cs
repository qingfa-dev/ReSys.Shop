using Microsoft.Extensions.Logging;

namespace Module.Payment.Domain.RefundReasons;

public static partial class RefundReasonLoggers
{
    [LoggerMessage(
        EventId = 3301,
        Level = LogLevel.Debug,
        Message = "[RefundReason.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3302,
        Level = LogLevel.Debug,
        Message = "[RefundReason.Activated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Activated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3303,
        Level = LogLevel.Debug,
        Message = "[RefundReason.Deactivated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deactivated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}
