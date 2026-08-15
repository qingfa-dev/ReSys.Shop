using Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Request : VariantPriceCollectionParameters;
}
