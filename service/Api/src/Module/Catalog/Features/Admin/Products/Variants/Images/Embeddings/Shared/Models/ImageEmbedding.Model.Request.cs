namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

public class CreateEmbeddingRequest
{
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
}

public record RegenerateEmbeddingRequest : ImageEmbeddingParameters;