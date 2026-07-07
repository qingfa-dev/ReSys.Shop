using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    public static T MapToDetail<T>(this ShippingMethod entity) where T : ShippingMethodDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            CalculatorType = entity.CalculatorType ?? string.Empty,
            Code = entity.Code,
            TaxCategoryId = entity.TaxCategoryId,
            TrackingUrl = entity.TrackingUrl,
            AdminName = entity.AdminName,
            Position = entity.Position,
            AvailableToUsers = entity.AvailableToUsers,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this ShippingMethod entity) where T : ShippingMethodListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            CalculatorType = entity.CalculatorType ?? string.Empty,
            Code = entity.Code,
            TaxCategoryId = entity.TaxCategoryId,
            TrackingUrl = entity.TrackingUrl,
            AdminName = entity.AdminName,
            Position = entity.Position,
            AvailableToUsers = entity.AvailableToUsers,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
