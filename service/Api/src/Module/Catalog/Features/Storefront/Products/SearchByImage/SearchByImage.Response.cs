namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    // EXCEPTION: search result collection — composite of Variant + embedding data
    public sealed record Response
    {
        public string? Message { get; init; }
        public List<SearchResultItem> Items { get; init; } = [];
    }

    public sealed record SearchResultItem
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
    }
}