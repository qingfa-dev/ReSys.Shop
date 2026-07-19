namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

public abstract record ImageEmbeddingParameters
{
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
}