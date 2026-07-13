using Module.Shipping.Domain.ShippingMethods;

using ShippingDomain = Module.Shipping.Domain.ShippingMethods.ShippingMethod;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    public static Result<ShippingDomain> MapToDomain<T>(this T request) where T : Models.ShippingMethodRequest
    {
        var result = ShippingMethodExtensions.Create(
            name: request.Name,
            calculatorType: request.CalculatorType,
            code: request.Code,
            taxCategoryId: request.TaxCategoryId);

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

    public static Result MapToDomain<T>(this T request, ShippingDomain method) where T : Models.ShippingMethodRequest
    {
        return method.Update(
            name: request.Name,
            code: request.Code,
            trackingUrl: request.TrackingUrl,
            adminName: request.AdminName,
            position: request.Position,
            availableToUsers: request.AvailableToUsers,
            calculatorType: request.CalculatorType,
            taxCategoryId: request.TaxCategoryId);
    }

    public static Result MapUpdateToDomain<T>(this T request, ShippingDomain method) where T : Models.ShippingMethodUpdateRequest
    {
        return method.Update(
            name: request.Name,
            code: request.Code,
            trackingUrl: request.TrackingUrl,
            adminName: request.AdminName,
            position: request.Position,
            availableToUsers: request.AvailableToUsers,
            calculatorType: request.CalculatorType,
            taxCategoryId: request.TaxCategoryId);
    }
}