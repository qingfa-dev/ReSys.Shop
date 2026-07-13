namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Request
    {
        public string ModelName { get; init; } = string.Empty;
    }
}