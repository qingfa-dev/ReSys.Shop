using Module.Catalog.Domain.Products;

namespace Module.Catalog.Features.Admin.Products.Shared.Models;

public record ProductDetailResponse : ProductParameters
{
    public Guid Id { get; init; }
    public ProductStatus Status { get; init; }
    public Guid MasterVariantId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

public record ProductListItemResponse : ProductParameters
{
    public Guid Id { get; init; }
    public ProductStatus Status { get; init; }
    public Guid MasterVariantId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public int VariantsCount { get; init; }
}