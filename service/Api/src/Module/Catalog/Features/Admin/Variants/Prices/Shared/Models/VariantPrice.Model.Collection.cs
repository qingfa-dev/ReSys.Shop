namespace Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

public abstract record VariantPriceCollectionParameters
{
    public Guid VariantId { get; init; }
    public List<PriceSyncItem> Prices { get; init; } = [];
}

public sealed record PriceSyncItem : PriceRequest;
