using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class StoreVariantPriceMapping
{
    public static T MapToStoreItem<T>(this Price item)
        where T : StoreVariantPriceListItemRepsonse, new()
    {
        var baseResponse = item.MapToDetail<T>();
        return baseResponse;
    }
}
