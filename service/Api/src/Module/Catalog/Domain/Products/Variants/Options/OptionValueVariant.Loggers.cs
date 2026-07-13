namespace Module.Catalog.Domain.Products.Variants.Options;

public static partial class OptionValueVariantLoggers
{
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Debug,
        Message = "[OptionValueVariant.Assigned]: Variant {VariantId}: {Count} option value(s) assigned")]
    public static partial void Assigned(ILogger logger, Guid VariantId, int Count);

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Debug,
        Message = "[OptionValueVariant.Revoked]: Variant {VariantId}: {Count} option value(s) revoked")]
    public static partial void Revoked(ILogger logger, Guid VariantId, int Count);

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Debug,
        Message = "[OptionValueVariant.Synced]: Variant {VariantId}: {Added} added, {Removed} removed")]
    public static partial void Synced(ILogger logger, Guid VariantId, int Added, int Removed);

    [LoggerMessage(
        EventId = 7004,
        Level = LogLevel.Debug,
        Message = "[OptionValueVariant.Listed]: Variant {VariantId}: {Count} option value(s)")]
    public static partial void Listed(ILogger logger, Guid VariantId, int Count);
}