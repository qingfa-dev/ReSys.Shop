namespace Module.Catalog.Domain.Products.Classifications;

public static partial class ClassificationLoggers
{
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Debug,
        Message = "[Classification.Assigned]: Product {ProductId}: {Count} taxon classification(s) assigned")]
    public static partial void Assigned(ILogger logger, Guid ProductId, int Count);

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Debug,
        Message = "[Classification.Revoked]: Product {ProductId}: {Count} taxon classification(s) revoked")]
    public static partial void Revoked(ILogger logger, Guid ProductId, int Count);

    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Debug,
        Message = "[Classification.Synced]: Product {ProductId}: {Added} added, {Removed} removed")]
    public static partial void Synced(ILogger logger, Guid ProductId, int Added, int Removed);
}