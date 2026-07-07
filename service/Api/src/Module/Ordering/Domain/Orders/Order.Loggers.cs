using Microsoft.Extensions.Logging;

namespace Module.Ordering.Domain.Orders;

// Log: Structured logging events for Order lifecycle operations
public static partial class OrderLoggers
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[Order.Created]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[Order.Placed]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Placed(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "[Order.Canceled]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Canceled(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[Order.Finalized]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Finalized(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Debug,
        Message = "[Order.Approved]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Approved(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Debug,
        Message = "[Order.Emptied]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Emptied(ILogger logger, string Number, Guid Id, string? ActionBy = "System");
}
