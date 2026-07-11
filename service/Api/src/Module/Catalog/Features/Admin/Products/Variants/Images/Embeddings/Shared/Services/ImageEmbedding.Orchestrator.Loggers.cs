namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

public sealed partial class EmbeddingOrchestrator
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 6001,
            Level = LogLevel.Information,
            Message = "Embedding persisted for VariantImageId={VariantImageId} Model={ModelName} Id={EmbeddingId}")]
        public static partial void EmbeddingPersisted(ILogger logger, Guid VariantImageId, string ModelName, Guid EmbeddingId);
    }
}
