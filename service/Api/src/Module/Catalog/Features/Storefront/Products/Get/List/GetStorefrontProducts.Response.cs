using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class GetStorefrontProducts
{
    public record Response : StoreProductListItemResponse
    {
        public FacetAggregate? Facets { get; init; }
    }

    public sealed record FacetAggregate(IReadOnlyList<FacetGroup> Groups);

    public sealed record FacetGroup(string Name, IReadOnlyList<FacetValue> Values);

    public sealed record FacetValue(string Id, string Label, int Count, bool IsActive);
}
