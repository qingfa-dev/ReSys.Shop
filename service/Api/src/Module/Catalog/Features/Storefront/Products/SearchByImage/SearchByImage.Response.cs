namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    public class Response
    {
        public string? Message { get; init; }
        public List<SearchResultItem> Items { get; init; } = [];
    }

    public class SearchResultItem
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
    }
}
