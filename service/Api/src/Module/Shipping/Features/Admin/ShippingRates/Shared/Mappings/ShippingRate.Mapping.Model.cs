using RateDomain = Module.Shipping.Domain.ShippingRates.ShippingRate;

namespace Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

public static partial class ShippingRateMapping
{
    public static T MapToDetail<T>(this RateDomain entity) where T : Models.ShippingRateDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Cost = entity.Cost,
            FinalPrice = entity.FinalPrice,
            Selected = entity.Selected,
            DeliveryRange = entity.DeliveryRange,
            MinWeight = entity.MinWeight,
            MaxWeight = entity.MaxWeight,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            ShippingMethodId = entity.ShippingMethodId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this RateDomain entity) where T : Models.ShippingRateListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Cost = entity.Cost,
            FinalPrice = entity.FinalPrice,
            Selected = entity.Selected,
            DeliveryRange = entity.DeliveryRange,
            MinWeight = entity.MinWeight,
            MaxWeight = entity.MaxWeight,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            ShippingMethodId = entity.ShippingMethodId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
