namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record VariantPriceActionParameters
{
    public Guid VariantId { get; init; }
    public Guid PriceId { get; init; }
}

public abstract record VariantPriceCollectionParameters
{
    public Guid VariantId { get; init; }
    public List<PriceSyncItem> Prices { get; init; } = [];
}

public sealed record PriceSyncItem : PriceRequest;
