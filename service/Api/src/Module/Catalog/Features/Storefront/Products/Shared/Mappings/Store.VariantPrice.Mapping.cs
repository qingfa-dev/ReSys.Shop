using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Variants.Prices.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreVariantPriceMapping
{
    public static T MapToStoreItem<T>(this Price item)
        where T : StoreVariantPriceListItemRepsonse, new()
    {
        var baseResponse = item.MapToDetail<T>();
        return baseResponse;
    }
}