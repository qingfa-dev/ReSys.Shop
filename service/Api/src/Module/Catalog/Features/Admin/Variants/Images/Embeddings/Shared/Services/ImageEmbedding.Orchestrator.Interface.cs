using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

public interface IEmbeddingOrchestrator
{
    Task<Result<EmbeddingDetailResponse>> GenerateAndPersistAsync(Guid variantImageId, string modelName, CancellationToken ct = default);

    Task<Result<EmbeddingDetailResponse>> GenerateAndPersistFromBytesAsync(Guid variantImageId, byte[] imageBytes, string contentType, string modelName, CancellationToken ct = default);

    Task<Result> RunAsync(Guid embeddingId, CancellationToken ct = default);
}