using Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Request
    {
        public List<SyncPriceItem> Prices { get; init; } = [];
    }

    public sealed record SyncPriceItem : PriceRequest;
}
