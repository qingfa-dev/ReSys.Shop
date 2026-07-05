namespace Module.Inventory.Domain.StockReservations;

public static partial class StockReservationLoggers
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[StockReservation.Reserved]: StockItem {StockItemId}, Quantity {Quantity}, Order {OrderId} by {ActionBy}")]
    public static partial void Reserved(ILogger logger, Guid StockItemId, int Quantity, Guid? OrderId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[StockReservation.Released]: StockItem {StockItemId} ({Id}) by {ActionBy}")]
    public static partial void Released(ILogger logger, Guid StockItemId, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Debug,
        Message = "[StockReservation.Extended]: StockItem {StockItemId} ({Id}) +{AdditionalMinutes}min by {ActionBy}")]
    public static partial void Extended(ILogger logger, Guid StockItemId, Guid Id, int AdditionalMinutes, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Warning,
        Message = "[StockReservation.Expired]: StockItem {StockItemId} ({Id})")]
    public static partial void Expired(ILogger logger, Guid StockItemId, Guid Id);
}
