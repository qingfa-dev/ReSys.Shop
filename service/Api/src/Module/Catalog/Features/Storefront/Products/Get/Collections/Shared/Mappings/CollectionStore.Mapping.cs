using Module.Catalog.Features.Storefront.Products.Get.Collections.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Collections.Shared.Mappings;

public static class CollectionStoreMapping
{
    public static T MapToStoreCollection<T>(this (string Season, string? StyleCode, int TotalProducts) source)
        where T : StoreCollectionPageResponse, new()
    {
        return new T
        {
            Season = source.Season,
            StyleCode = source.StyleCode,
            TotalProducts = source.TotalProducts,
        };
    }
}
