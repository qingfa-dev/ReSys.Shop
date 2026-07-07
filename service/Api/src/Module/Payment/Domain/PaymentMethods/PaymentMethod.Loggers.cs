using Microsoft.Extensions.Logging;

namespace Module.Payment.Domain.PaymentMethods;

public static partial class PaymentMethodLoggers
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[PaymentMethod.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[PaymentMethod.Activated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Activated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[PaymentMethod.Deactivated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deactivated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[PaymentMethod.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}