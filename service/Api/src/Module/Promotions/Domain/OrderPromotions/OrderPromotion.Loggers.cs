using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.OrderPromotions;

public static partial class OrderPromotionLoggers
{
    [LoggerMessage(
        EventId = 3501,
        Level = LogLevel.Debug,
        Message = "[OrderPromotion.Created]: Order {OrderId} — Promotion {PromotionId} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, Guid OrderId, Guid PromotionId, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3502,
        Level = LogLevel.Debug,
        Message = "[OrderPromotion.Deleted]: Order {OrderId} — Promotion {PromotionId} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, Guid OrderId, Guid PromotionId, Guid Id, string? ActionBy = "System");
}
