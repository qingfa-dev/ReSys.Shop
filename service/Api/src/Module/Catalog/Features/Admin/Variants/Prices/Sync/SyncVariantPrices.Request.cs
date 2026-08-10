using Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public List<SyncPriceItem> Prices { get; init; } = [];
    }

    public sealed record SyncPriceItem : PriceRequest;
}
