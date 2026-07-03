using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Collections;

public static partial class GetCollectionPage
{
public record Response : StoreProductListItemResponse
    {
        public string Season { get; init; } = string.Empty;
    }
}
