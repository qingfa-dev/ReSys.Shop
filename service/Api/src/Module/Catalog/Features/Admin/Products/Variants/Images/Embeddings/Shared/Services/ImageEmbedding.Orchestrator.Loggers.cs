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

        [LoggerMessage(
            EventId = 6002,
            Level = LogLevel.Warning,
            Message = "VariantImageId={VariantImageId} was deleted before embedding could be persisted. Model={ModelName}")]
        public static partial void VariantImageDeletedDuringEmbedding(ILogger logger, Guid VariantImageId, string ModelName);
    }
}