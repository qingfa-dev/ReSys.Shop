namespace Module.Inventory.Domain.StockItems;

public static partial class StockItemLoggers
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[StockItem.Created]: Variant '{VariantId}' at Location '{StockLocationId}' ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, Guid VariantId, Guid StockLocationId, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[StockItem.Adjusted]: CountOnHand adjusted to {CountOnHand} ({Id}) by {ActionBy}")]
    public static partial void Adjusted(ILogger logger, int CountOnHand, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Debug,
        Message = "[StockItem.Picked]: Picked {Quantity} units from {Id} by {ActionBy}")]
    public static partial void Picked(ILogger logger, int Quantity, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Debug,
        Message = "[StockItem.Restocked]: Restocked {Quantity} units to {Id} by {ActionBy}")]
    public static partial void Restocked(ILogger logger, int Quantity, Guid Id, string? ActionBy = "System");
}