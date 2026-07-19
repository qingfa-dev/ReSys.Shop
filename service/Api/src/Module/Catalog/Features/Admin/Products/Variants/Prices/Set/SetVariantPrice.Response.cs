namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Set;

public static partial class SetVariantPrice
{
    // EXCEPTION: minimal confirmation response — no domain entity
    public sealed record Response
    {
        public Guid VariantId { get; init; }
    }
}