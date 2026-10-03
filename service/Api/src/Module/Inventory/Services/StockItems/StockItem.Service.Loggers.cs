namespace Module.Inventory.Services;

internal sealed partial class StockItemService
{
    internal static partial class Loggers
    {
        [LoggerMessage(EventId = 3100, Level = LogLevel.Information, Message = "Stock adjusted: variant={VariantId}, delta={Delta}, new count={NewCount}")]
        public static partial void LogStockAdjusted(ILogger logger, Guid variantId, int delta, int newCount);

        [LoggerMessage(EventId = 3101, Level = LogLevel.Warning, Message = "Stock adjustment failed: variant={VariantId}, delta={Delta}, reason={Reason}")]
        public static partial void LogStockAdjustmentFailed(ILogger logger, Guid variantId, int delta, string reason);

        [LoggerMessage(EventId = 3102, Level = LogLevel.Information, Message = "Restocked item {StockItemId}: +{Quantity}, backorders fulfilled={BackordersFulfilled}")]
        public static partial void LogRestocked(ILogger logger, Guid stockItemId, int quantity, int backordersFulfilled);

        [LoggerMessage(EventId = 3103, Level = LogLevel.Warning, Message = "Restock failed for item {StockItemId}: {Reason}")]
        public static partial void LogRestockFailed(ILogger logger, Guid stockItemId, string reason);
    }
}
