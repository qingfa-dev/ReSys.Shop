using Module.Shipping.Domain.ShippingMethods;
using Shared.Application.Domain.Currencies;

namespace Module.Shipping.Features.Storefront.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    public static T MapToDetail<T>(this ShippingMethod entity) where T : Storefront.Shared.Models.ShippingMethodDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            MethodName = entity.Name,
            Description = entity.AdminName ?? string.Empty,
            Cost = 0m,
            Currency = SystemCurrencyConstant.Defaults.Code,
            IsActive = entity.AvailableToUsers,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this ShippingMethod entity) where T : Storefront.Shared.Models.ShippingMethodListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            MethodName = entity.Name,
            Description = entity.AdminName ?? string.Empty,
            Cost = 0m,
            Currency = SystemCurrencyConstant.Defaults.Code,
            IsActive = entity.AvailableToUsers,
        };
    }
}