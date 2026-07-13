namespace Module.Ordering.Domain.Adjustments;

// Log: Structured logging events for Adjustment lifecycle operations
public static partial class AdjustmentLoggers
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[Adjustment.Created]: {Label} ({Id}) on Order {OrderId} by {ActionBy}")]
    public static partial void Created(ILogger logger, string Label, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[Adjustment.Closed]: {Label} ({Id}) on Order {OrderId} by {ActionBy}")]
    public static partial void Closed(ILogger logger, string Label, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[Adjustment.Opened]: {Label} ({Id}) on Order {OrderId} by {ActionBy}")]
    public static partial void Opened(ILogger logger, string Label, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[Adjustment.MarkedIneligible]: {Label} ({Id}) on Order {OrderId} by {ActionBy}")]
    public static partial void MarkedIneligible(ILogger logger, string Label, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Debug,
        Message = "[Adjustment.MarkedEligible]: {Label} ({Id}) on Order {OrderId} by {ActionBy}")]
    public static partial void MarkedEligible(ILogger logger, string Label, Guid Id, Guid OrderId, string? ActionBy = "System");
}