namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public sealed record Request
    {
        public string ModelName { get; init; } = string.Empty;
        public string ModelVersion { get; init; } = string.Empty;
    }
}