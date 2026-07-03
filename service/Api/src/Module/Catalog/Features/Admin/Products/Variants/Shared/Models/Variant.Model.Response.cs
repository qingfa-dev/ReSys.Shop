namespace Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

public record VariantDetailResponse : VariantParameters
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public bool IsMaster { get; init; }
    public DateTimeOffset? DiscontinuedOn { get; init; }
    public int PricesCount { get; init; }
}

public record VariantListItemResponse : VariantParameters
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public bool IsMaster { get; init; }
}
