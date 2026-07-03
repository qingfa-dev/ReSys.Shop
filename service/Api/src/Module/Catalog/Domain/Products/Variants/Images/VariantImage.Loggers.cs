namespace Module.Catalog.Domain.Products.Variants.Images;

public static partial class VariantImageLoggers
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[VariantImage.Created]: File '{FileName}' ({Id}) for Variant {VariantId} by {ActionBy}")]
    public static partial void Created(ILogger logger, Guid Id, Guid VariantId, string FileName, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[VariantImage.Updated]: ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Debug,
        Message = "[VariantImage.Deleted]: ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, Guid Id, string? ActionBy = "System");
}
