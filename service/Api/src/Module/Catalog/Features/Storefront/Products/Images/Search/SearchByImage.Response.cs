namespace Module.Catalog.Features.Storefront.Products.Images.Search;

public static partial class SearchByImage
{
    // EXCEPTION: search-result DTO — composite of Variant + embedding data
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
        public double SimilarityScore { get; init; }
    }
}
