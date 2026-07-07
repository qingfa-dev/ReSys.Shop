using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Shared.Mappings;

public static partial class MethodRateMapping
{
    public static T MapToDetail<T>(this ShippingRate entity) where T : MethodRateDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Cost = entity.Cost,
            FinalPrice = entity.FinalPrice,
            DeliveryRange = entity.DeliveryRange,
            MinWeight = entity.MinWeight,
            MaxWeight = entity.MaxWeight,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            ShippingMethodId = entity.ShippingMethodId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    public static T MapToListItem<T>(this ShippingRate entity) where T : MethodRateListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Cost = entity.Cost,
            FinalPrice = entity.FinalPrice,
            DeliveryRange = entity.DeliveryRange,
            MinWeight = entity.MinWeight,
            MaxWeight = entity.MaxWeight,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            ShippingMethodId = entity.ShippingMethodId,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
