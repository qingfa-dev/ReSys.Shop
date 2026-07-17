using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

public record StoreProductDetailResponse : ProductDetailResponse
{
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductVariantResponse> Variants { get; init; } = [];
    public List<StoreProductImageResponse> Images { get; init; } = [];
    public List<StoreProductTaxonResponse> Taxons { get; init; } = [];
}

public record StoreProductListItemResponse : ProductListItemResponse
{
    public decimal? MinPrice { get; init; }
    public string? Currency { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? ThumbnailAlt { get; init; }
}

public class StoreProductVariantResponse
{
    public Guid Id { get; init; }
    public string? Sku { get; init; }
    public bool IsMaster { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public string? OptionValue1 { get; init; }
    public string? OptionValue2 { get; init; }
    public List<StoreProductImageResponse> Images { get; init; } = [];
}

public class StoreProductImageResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? Alt { get; init; }
    public int Position { get; init; }
    public string ContentType { get; init; } = string.Empty;
}

public class StoreProductTaxonResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Permalink { get; init; } = string.Empty;
    public int Depth { get; init; }
}