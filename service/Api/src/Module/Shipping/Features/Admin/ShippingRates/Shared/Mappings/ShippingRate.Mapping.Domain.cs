using Module.Shipping.Domain.ShippingRates;

using RateDomain = Module.Shipping.Domain.ShippingRates.ShippingRate;

namespace Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

public static partial class ShippingRateMapping
{
    public static Result<RateDomain> MapToDomain<T>(this T request) where T : Models.ShippingRateRequest
    {
        return ShippingRateExtensions.Create(
            name: request.Name,
            cost: request.Cost,
            shippingMethodId: request.ShippingMethodId,
            deliveryRange: request.DeliveryRange,
            minWeight: request.MinWeight,
            maxWeight: request.MaxWeight,
            freeShippingThreshold: request.FreeShippingThreshold);
    }

    public static Result MapToDomain<T>(this T request, RateDomain rate) where T : Models.ShippingRateRequest
    {
        return rate.Update(
            name: request.Name,
            cost: request.Cost,
            deliveryRange: request.DeliveryRange,
            minWeight: request.MinWeight,
            maxWeight: request.MaxWeight,
            freeShippingThreshold: request.FreeShippingThreshold,
            shippingMethodId: request.ShippingMethodId);
    }

    
}