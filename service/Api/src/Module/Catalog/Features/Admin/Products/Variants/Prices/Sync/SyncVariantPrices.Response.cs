namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Response
    {
        public int Added { get; init; }
        public int Updated { get; init; }
        public int Removed { get; init; }
    }
}
