namespace Module.Catalog.Features.Admin.Products.Variants.Images.Delete;

public static partial class DeleteVariantImage
{
    // EXCEPTION: minimal confirmation response — no domain entity
    public sealed record Response
    {
        public string Message { get; init; } = default!;
    }
}