using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

public static partial class ShippingMethodMapping
{
    /// <summary>Maps a request to a new ShippingMethod domain entity.</summary>
    public static Result<ShippingMethod> MapToDomain<T>(this T request) where T : ShippingMethodRequest
    {
        return ShippingMethodExtensions.Create(
            name: request.Name,
            calculatorType: request.CalculatorType,
            code: request.Code,
            taxCategoryId: request.TaxCategoryId);
    }

    /// <summary>Maps a PATCH update request to update an existing ShippingMethod.</summary>
    public static Result MapUpdateToDomain<T>(this T request, ShippingMethod method) where T : ShippingMethodUpdateRequest
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
