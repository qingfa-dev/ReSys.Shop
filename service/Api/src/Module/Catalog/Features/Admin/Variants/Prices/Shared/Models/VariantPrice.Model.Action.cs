namespace Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

public abstract record VariantPriceActionParameters
{
    public Guid VariantId { get; init; }
    public Guid PriceId { get; init; }
}
