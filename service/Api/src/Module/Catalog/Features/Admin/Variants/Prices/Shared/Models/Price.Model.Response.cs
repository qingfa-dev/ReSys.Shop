namespace Module.Catalog.Features.Admin.Variants.Prices.Shared.Models;

public record PriceResponse : PriceParameters
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
}