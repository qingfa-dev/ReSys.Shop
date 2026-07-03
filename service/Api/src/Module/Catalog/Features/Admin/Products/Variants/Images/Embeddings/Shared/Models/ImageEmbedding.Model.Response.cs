namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

public class EmbeddingDetailResponse
{
    public Guid Id { get; init; }
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public float[] Vector { get; init; } = [];
    public int Dimensions { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
