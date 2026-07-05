namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public interface IInferenceClient
{
    Task<Result<EmbeddingResponse>> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken ct = default);

    Task<Result<EmbeddingResponse>> CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? model = null, CancellationToken ct = default);

    Task<Result<List<ModelMetadata>>> ListModelsAsync(CancellationToken ct = default);
}
