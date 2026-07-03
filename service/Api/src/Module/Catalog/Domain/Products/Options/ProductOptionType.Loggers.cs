namespace Module.Catalog.Domain.Products.Options;

public static partial class ProductOptionTypeLoggers
{
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Debug,
        Message = "[ProductOptionType.Assigned]: Product {ProductId}: {Count} option type(s) assigned")]
    public static partial void Assigned(ILogger logger, Guid ProductId, int Count);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Debug,
        Message = "[ProductOptionType.Revoked]: Product {ProductId}: {Count} option type(s) revoked")]
    public static partial void Revoked(ILogger logger, Guid ProductId, int Count);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Debug,
        Message = "[ProductOptionType.Synced]: Product {ProductId}: {Added} added, {Removed} removed")]
    public static partial void Synced(ILogger logger, Guid ProductId, int Added, int Removed);
}
