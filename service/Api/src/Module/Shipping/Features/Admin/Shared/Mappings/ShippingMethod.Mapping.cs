using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.Shared.Models;


namespace Module.Shipping.Features.Admin.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    #region Domain
    public static Result<ShippingMethod> MapToDomain<T>(this T request) where T : ShippingMethodRequest
    {
        var result = ShippingMethodMethod.Create(
            name: request.Name,
            calculatorType: request.CalculatorType,
            code: request.Code);

        if (result.IsFailure)
            return result.Errors;

        var method = result.Value;
        method.TrackingUrl = request.TrackingUrl;
        method.AdminName = request.AdminName;
        method.Position = request.Position;
        method.AvailableToUsers = request.AvailableToUsers;
        method.Presentation = request.Presentation;

        return method;
    }

    public static Result MapToDomain<T>(this T request, ShippingMethod method) where T : ShippingMethodRequest
    {
        return method.Update(
            name: request.Name,
            code: request.Code,
            trackingUrl: request.TrackingUrl,
            adminName: request.AdminName,
            position: request.Position,
            availableToUsers: request.AvailableToUsers,
            calculatorType: request.CalculatorType);
    }
    #endregion

    #region Models
    public static T MapToDetail<T>(this ShippingMethod entity) where T : ShippingMethodDetailResponse, new()
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
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
            IsDeleted = entity.IsDeleted,
            DeletedAtUtc = entity.DeletedAtUtc,
        };
    }

    public static T MapToListItem<T>(this ShippingMethod entity) where T : ShippingMethodListItemResponse, new()
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
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
    #endregion

}