namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public sealed record Request
    {
        public Guid VariantImageId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
    }
}