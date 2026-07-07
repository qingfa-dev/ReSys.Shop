using Microsoft.Extensions.Logging;

namespace Module.Shipping.Domain.ShippingRates;

// Log: Source-generated loggers for shipping rate lifecycle events (CAT-9 Observability)
public static partial class ShippingRateLoggers
{
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Debug,
        Message = "[ShippingRate.Created]: {Name} ({Id}) for Shipment '{ShipmentId}' by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, Guid ShipmentId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Debug,
        Message = "[ShippingRate.Selected]: {Name} ({Id}) was selected by {ActionBy}")]
    public static partial void Selected(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Debug,
        Message = "[ShippingRate.Unselected]: {Name} ({Id}) was unselected by {ActionBy}")]
    public static partial void Unselected(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
}