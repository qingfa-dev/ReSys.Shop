namespace Module.Shipping.Domain.ShippingMethods;

// Log: Source-generated loggers for shipping method lifecycle events (CAT-9 Observability)
public static partial class ShippingMethodLoggers
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[ShippingMethod.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "[ShippingMethod.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "[ShippingMethod.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}