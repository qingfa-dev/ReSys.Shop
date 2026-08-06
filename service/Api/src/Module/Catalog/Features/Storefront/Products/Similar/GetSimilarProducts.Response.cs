namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    // EXCEPTION: search-result DTO — composite of variant + product data, no domain entity
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}
