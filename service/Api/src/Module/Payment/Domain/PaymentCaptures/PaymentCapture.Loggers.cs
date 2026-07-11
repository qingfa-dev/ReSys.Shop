namespace Module.Payment.Domain.PaymentCaptures;

public static partial class PaymentCaptureLoggers
{
    [LoggerMessage(
        EventId = 3101,
        Level = LogLevel.Debug,
        Message = "[Payment.Created]: {Number} ({Id}) for Order {OrderId} by {ActionBy}")]
    public static partial void Created(ILogger logger, string Number, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3102,
        Level = LogLevel.Debug,
        Message = "[Payment.Completed]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Completed(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3103,
        Level = LogLevel.Debug,
        Message = "[Payment.Voided]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Voided(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3104,
        Level = LogLevel.Debug,
        Message = "[Payment.Failed]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Failed(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3105,
        Level = LogLevel.Debug,
        Message = "[Payment.Captured]: {Number} ({Id}) for {Amount} by {ActionBy}")]
    public static partial void Captured(ILogger logger, string Number, Guid Id, decimal Amount, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Warning,
        Message = "[Payment.DisputeCreated]: Charge {ChargeId} reason {Reason}")]
    public static partial void DisputeCreated(this ILogger logger, string chargeId, string reason);
}