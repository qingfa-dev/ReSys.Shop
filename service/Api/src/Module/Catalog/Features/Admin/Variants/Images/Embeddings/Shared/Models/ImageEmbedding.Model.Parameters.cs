namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models;

public abstract record ImageEmbeddingParameters
{
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
}