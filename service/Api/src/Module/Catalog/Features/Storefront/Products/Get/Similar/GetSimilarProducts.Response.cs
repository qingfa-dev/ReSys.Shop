namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    // EXCEPTION: search-result collection wrapper — no domain entity
    public sealed record Response
    {
        public List<SimilarProductItem> Items { get; init; } = [];
    }

    public sealed record SimilarProductItem
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}