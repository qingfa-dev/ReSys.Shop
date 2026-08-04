namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    // EXCEPTION: minimal confirmation response — no domain entity
    public sealed record Response
    {
        public string Message { get; init; } = default!;
    }
}
