namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Request
    {
        public List<SyncPriceItem> Prices { get; init; } = [];
    }

    public sealed record SyncPriceItem
    {
        public decimal? Amount { get; init; }
        public string Currency { get; init; } = null!;
        public decimal? CompareAtAmount { get; init; }
        public string? CountryIso { get; init; }
    }
}