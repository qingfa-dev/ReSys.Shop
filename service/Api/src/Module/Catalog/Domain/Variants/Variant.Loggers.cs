namespace Module.Catalog.Domain.Variants;

public static partial class VariantLoggers
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[Variant.Created]: Sku '{Sku}' ({Id}) for Product {ProductId} by {ActionBy}")]
    public static partial void Created(ILogger logger, string Sku, Guid Id, Guid ProductId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[Variant.Updated]: Sku '{Sku}' ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Sku, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[Variant.Deleted]: Sku '{Sku}' ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Sku, Guid Id, string? ActionBy = "System");
}