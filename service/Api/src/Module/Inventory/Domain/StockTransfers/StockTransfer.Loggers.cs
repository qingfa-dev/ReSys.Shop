namespace Module.Inventory.Domain.StockTransfers;

public static partial class StockTransferLoggers
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Stock transfer {Id} created from {SourceId} to {DestinationId}")]
    public static partial void Created(ILogger logger, Guid Id, Guid SourceId, Guid DestinationId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Stock transfer {Id} is now in transit")]
    public static partial void Transferred(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Stock transfer {Id} received at destination")]
    public static partial void Received(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Stock transfer {Id} canceled")]
    public static partial void Canceled(ILogger logger, Guid Id);
}