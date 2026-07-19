namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;

public record PriceResponse : PriceParameters, IResponse
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
}