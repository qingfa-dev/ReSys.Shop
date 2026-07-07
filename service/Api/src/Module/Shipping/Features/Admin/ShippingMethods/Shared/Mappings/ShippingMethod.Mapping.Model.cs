using ShippingDomain = Module.Shipping.Domain.ShippingMethods.ShippingMethod;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    public static T MapToDetail<T>(this ShippingDomain entity) where T : Models.ShippingMethodDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            TrackingUrl = entity.TrackingUrl,
            AdminName = entity.AdminName,
            Position = entity.Position,
            AvailableToUsers = entity.AvailableToUsers,
            CalculatorType = entity.CalculatorType ?? string.Empty,
            TaxCategoryId = entity.TaxCategoryId,
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
            IsDeleted = entity.IsDeleted,
            DeletedAtUtc = entity.DeletedAtUtc,
        };
    }

    public static T MapToListItem<T>(this ShippingDomain entity) where T : Models.ShippingMethodListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            TrackingUrl = entity.TrackingUrl,
            AdminName = entity.AdminName,
            Position = entity.Position,
            AvailableToUsers = entity.AvailableToUsers,
            CalculatorType = entity.CalculatorType ?? string.Empty,
            TaxCategoryId = entity.TaxCategoryId,
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
