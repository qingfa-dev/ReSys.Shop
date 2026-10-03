namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

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

        [LoggerMessage(
            EventId = 6003,
            Level = LogLevel.Information,
            Message = "RunAsync started EmbeddingId={EmbeddingId} VariantImageId={VariantImageId} Model={ModelName}")]
        public static partial void RunStarted(ILogger logger, Guid EmbeddingId, Guid VariantImageId, string ModelName);

        [LoggerMessage(
            EventId = 6004,
            Level = LogLevel.Information,
            Message = "RunAsync processing EmbeddingId={EmbeddingId}")]
        public static partial void RunProcessing(ILogger logger, Guid EmbeddingId);

        [LoggerMessage(
            EventId = 6005,
            Level = LogLevel.Information,
            Message = "RunAsync completed EmbeddingId={EmbeddingId} Dimensions={Dimensions}")]
        public static partial void RunCompleted(ILogger logger, Guid EmbeddingId, int Dimensions);

        [LoggerMessage(
            EventId = 6006,
            Level = LogLevel.Error,
            Message = "RunAsync failed EmbeddingId={EmbeddingId} Error={Error}")]
        public static partial void RunFailed(ILogger logger, Guid EmbeddingId, string Error);
    }
}