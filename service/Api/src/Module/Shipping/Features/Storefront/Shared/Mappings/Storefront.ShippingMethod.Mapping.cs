using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.Shared.Mappings;
using Module.Shipping.Features.Storefront.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shared.Mappings;

public static partial class StorefrontShippingMethodMapping
{
    public static T MapToStorefrontListItem<T>(this ShippingMethod entity) where T : StorefrontShippingMethodListItem, new()
    {
        var baseResponse = entity.MapToListItem<T>();

        return baseResponse;
    }
}