using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.PromotionActions;

public static partial class PromotionActionLoggers
{
    [LoggerMessage(
        EventId = 3301,
        Level = LogLevel.Debug,
        Message = "[PromotionAction.Created]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Type, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3302,
        Level = LogLevel.Debug,
        Message = "[PromotionAction.Updated]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Type, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3303,
        Level = LogLevel.Debug,
        Message = "[PromotionAction.Deleted]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Type, Guid Id, string? ActionBy = "System");
}
