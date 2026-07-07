using Microsoft.Extensions.Logging;

namespace Module.Promotions.Domain.CouponCodes;

public static partial class CouponCodeLoggers
{
    [LoggerMessage(
        EventId = 3101,
        Level = LogLevel.Debug,
        Message = "[CouponCode.Created]: {Code} ({Id}) for Promotion {PromotionId} by {ActionBy}")]
    public static partial void Created(ILogger logger, string Code, Guid Id, Guid PromotionId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3102,
        Level = LogLevel.Debug,
        Message = "[CouponCode.Redeemed]: {Code} ({Id}) for Order {OrderId} by {ActionBy}")]
    public static partial void Redeemed(ILogger logger, string Code, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3103,
        Level = LogLevel.Debug,
        Message = "[CouponCode.Expired]: {Code} ({Id}) by {ActionBy}")]
    public static partial void Expired(ILogger logger, string Code, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3104,
        Level = LogLevel.Debug,
        Message = "[CouponCode.Canceled]: {Code} ({Id}) by {ActionBy}")]
    public static partial void Canceled(ILogger logger, string Code, Guid Id, string? ActionBy = "System");
}