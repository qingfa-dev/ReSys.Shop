using Microsoft.Extensions.Logging;

namespace Module.Shipping.Domain.Shipments;

// Log: Source-generated loggers for shipment lifecycle events (CAT-9 Observability)
public static partial class ShipmentLoggers
{
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Debug,
        Message = "[Shipment.Created]: {Number} ({Id}) for Order '{OrderId}' by {ActionBy}")]
    public static partial void Created(ILogger logger, string Number, Guid Id, Guid OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Debug,
        Message = "[Shipment.Ready]: {Number} ({Id}) is ready for pickup by {ActionBy}")]
    public static partial void Ready(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Information,
        Message = "[Shipment.Shipped]: {Number} ({Id}) with tracking '{Tracking}' by {ActionBy}")]
    public static partial void Shipped(ILogger logger, string Number, Guid Id, string Tracking, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6004,
        Level = LogLevel.Information,
        Message = "[Shipment.Canceled]: {Number} ({Id}) by {ActionBy}")]
    public static partial void Canceled(ILogger logger, string Number, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6005,
        Level = LogLevel.Debug,
        Message = "[Shipment.RateSelected]: Rate selected for shipment {Number} ({Id}) by {ActionBy}")]
    public static partial void RateSelected(ILogger logger, string Number, Guid Id, string? ActionBy = "System");
}