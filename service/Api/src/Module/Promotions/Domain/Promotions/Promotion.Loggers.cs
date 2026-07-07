using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.Promotions;

public static partial class PromotionLoggers
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[Promotion.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[Promotion.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[Promotion.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[Promotion.Activated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Activated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Debug,
        Message = "[Promotion.Deactivated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deactivated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}