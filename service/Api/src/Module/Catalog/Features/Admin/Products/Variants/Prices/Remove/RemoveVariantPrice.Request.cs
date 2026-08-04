namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Remove;

public static partial class RemoveVariantPrice
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public Guid PriceId { get; init; }
    }
}
