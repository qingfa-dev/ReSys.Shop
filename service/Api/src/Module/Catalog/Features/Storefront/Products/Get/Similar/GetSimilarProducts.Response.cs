using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    /// <summary>Product list item enriched with the visual similarity score.</summary>
    public sealed record Response : StoreProductListItemResponse
    {
        public double SimilarityScore { get; init; }
    }
}