namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItemsLoggers
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[StockItem.Import]: Created {Created}, Updated {Updated}, Failed {Failed}")]
    public static partial void ImportCompleted(ILogger logger, int Created, int Updated, int Failed);
}
