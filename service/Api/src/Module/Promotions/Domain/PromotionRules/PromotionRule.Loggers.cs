using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.PromotionRules;

public static partial class PromotionRuleLoggers
{
    [LoggerMessage(
        EventId = 3401,
        Level = LogLevel.Debug,
        Message = "[PromotionRule.Created]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Type, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3402,
        Level = LogLevel.Debug,
        Message = "[PromotionRule.Updated]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Type, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3403,
        Level = LogLevel.Debug,
        Message = "[PromotionRule.Deleted]: {Type} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Type, Guid Id, string? ActionBy = "System");
}
