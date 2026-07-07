using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Shared.Mappings;

public static partial class MethodRateMapping
{
    /// <summary>Maps a request to a new ShippingRate domain entity.</summary>
    public static Result<ShippingRate> MapToDomain<T>(this T request, Guid methodId) where T : MethodRateRequest
    {
        return ShippingRateExtensions.Create(
            name: request.Name,
            cost: request.Cost,
            shipmentId: Guid.Empty,
            shippingMethodId: methodId,
            deliveryRange: request.DeliveryRange,
            minWeight: request.MinWeight,
            maxWeight: request.MaxWeight,
            freeShippingThreshold: request.FreeShippingThreshold);
    }
}
