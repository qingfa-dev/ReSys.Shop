namespace Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

public record PriceRequest : PriceParameters
{
    public Guid VariantId { get; init; }
}