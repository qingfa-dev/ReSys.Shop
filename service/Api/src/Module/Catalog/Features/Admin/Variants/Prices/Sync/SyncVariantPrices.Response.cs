using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public record Response : PriceResponse
    {
        public int Added { get; init; }
        public int Updated { get; init; }
        public int Removed { get; init; }
    }
}