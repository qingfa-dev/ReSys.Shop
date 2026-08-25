namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;

public interface IInferenceClient
{
    Task<Result<EmbeddingResponse>> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken ct = default);

    Task<Result<EmbeddingResponse>> CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? modelName = null, CancellationToken ct = default);

    Task<Result<List<ModelMetadata>>> ListModelsAsync(CancellationToken ct = default);
}