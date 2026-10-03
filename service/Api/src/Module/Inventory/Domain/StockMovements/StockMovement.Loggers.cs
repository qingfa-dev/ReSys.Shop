namespace Module.Inventory.Domain.StockMovements;

public static partial class StockMovementLoggers
{
    [LoggerMessage(
        EventId = 4101,
        Level = LogLevel.Debug,
        Message = "[StockMovement.Recorded]: StockItem {StockItemId}, Quantity {Quantity} by {ActionBy}")]
    public static partial void Recorded(ILogger logger, Guid StockItemId, int Quantity, string? ActionBy = "System");
}