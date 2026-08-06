namespace Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

public record VariantListItemResponse : VariantParameters
{
    public Guid Id { get; init; }
    public bool IsMaster { get; init; }
}

public record VariantDetailResponse : VariantListItemResponse
{
    public DateTimeOffset? DiscontinuedOn { get; init; }
    public int PricesCount { get; init; }
}

