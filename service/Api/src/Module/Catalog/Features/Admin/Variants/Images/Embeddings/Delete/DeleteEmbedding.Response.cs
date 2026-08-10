namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    // EXCEPTION: minimal confirmation response — no domain entity
    public sealed record Response
    {
        public string Message { get; init; } = default!;
    }
}
