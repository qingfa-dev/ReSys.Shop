namespace Module.Catalog.Features.Storefront.Products.Get.Search.Shared.Models;

public class StoreProductSearchResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MinPrice { get; init; }
    public string? Currency { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? ThumbnailAlt { get; init; }
    public double? Score { get; init; }
}
