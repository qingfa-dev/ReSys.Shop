using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

public interface IEmbeddingOrchestrator
{
    Task<Result<EmbeddingDetailResponse>> GenerateAndPersistAsync(Guid variantImageId, string modelName, CancellationToken ct = default);

    Task<Result<EmbeddingDetailResponse>> GenerateAndPersistFromBytesAsync(Guid variantImageId, byte[] imageBytes, string contentType, string modelName, CancellationToken ct = default);
}
