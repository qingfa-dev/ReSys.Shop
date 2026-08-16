using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shared.Mappings;

public static class ShippingRateMapping
{
      public static Result<ShippingRate> MapToDomain<T>(this T request) where T : ShippingRateRequest
    {
        return ShippingRateMethod.Create(
            name: request.Name,
            cost: request.Cost,
            shippingMethodId: request.ShippingMethodId,
            deliveryRange: request.DeliveryRange,
            minWeight: request.MinWeight,
            maxWeight: request.MaxWeight,
            freeShippingThreshold: request.FreeShippingThreshold);
    }

    public static Result MapToDomain<T>(this T request, ShippingRate rate) where T : ShippingRateRequest
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
    public static T MapToDetail<T>(this ShippingRate entity) where T : ShippingRateDetailResponse, new()
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

    public static T MapToListItem<T>(this ShippingRate entity) where T : Models.ShippingRateListItemResponse, new()
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