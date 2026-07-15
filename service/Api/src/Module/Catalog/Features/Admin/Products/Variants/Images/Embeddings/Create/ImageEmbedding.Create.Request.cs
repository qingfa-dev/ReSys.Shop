namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Request
    {
        public Guid VariantImageId { get; init; }
        public required string ModelName { get; init; }
    }
}