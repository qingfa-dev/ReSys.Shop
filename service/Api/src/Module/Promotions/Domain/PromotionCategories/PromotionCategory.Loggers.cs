using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.PromotionCategories;

public static partial class PromotionCategoryLoggers
{
    [LoggerMessage(
        EventId = 3201,
        Level = LogLevel.Debug,
        Message = "[PromotionCategory.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3202,
        Level = LogLevel.Debug,
        Message = "[PromotionCategory.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3203,
        Level = LogLevel.Debug,
        Message = "[PromotionCategory.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}
