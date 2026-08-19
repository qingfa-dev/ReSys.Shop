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

    [LoggerMessage(
        EventId = 3010,
        Level = LogLevel.Warning,
        Message = "Failed to void payments for order {OrderId}: {Errors}")]
    public static partial void VoidPaymentsFailed(ILogger logger, Guid OrderId, string Errors);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Warning,
        Message = "Failed to send order canceled notification for order {OrderId}: {Errors}")]
    public static partial void CancelNotificationFailed(ILogger logger, Guid OrderId, string Errors);

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Warning,
        Message = "Failed to send order confirmation notification for order {OrderId}: {Errors}")]
    public static partial void ConfirmationNotificationFailed(ILogger logger, Guid OrderId, string Errors);

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Warning,
        Message = "Failed to send order resumed notification for order {OrderId}: {Errors}")]
    public static partial void ResumeNotificationFailed(ILogger logger, Guid OrderId, string Errors);

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Warning,
        Message = "Failed to auto-complete order {OrderId}: {Errors}")]
    public static partial void CompletionFailed(ILogger logger, Guid OrderId, string Errors);
}