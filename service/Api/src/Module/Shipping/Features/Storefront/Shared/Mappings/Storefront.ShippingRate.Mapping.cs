using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.Shared.Mappings;
using Module.Shipping.Features.Storefront.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shared.Mappings;

public static class StorefrontShippingRateMapping
{
    public static T MapToStorefrontListItem<T>(this ShippingRate entity) where T : StorefrontShippingRateResponse, new()
    {
        var baseResponse = entity.MapToListItem<T>();

        return baseResponse;
    }
}