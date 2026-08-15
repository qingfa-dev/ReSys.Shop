using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Mappings;

public static class CalculateShippingMapping
{
    public static T MapToResponse<T>(this (ShippingMethod Method, string Currency, decimal Cost, bool IsFreeShipping, Guid ShippingRateId) source)
        where T : ShippingCalculationParameters, new()
        => new T
        {
            ShippingMethodId = source.Method.Id,
            MethodName = source.Method.Name ?? string.Empty,
            Cost = source.Cost,
            Currency = source.Currency,
            IsFreeShipping = source.IsFreeShipping,
            ShippingRateId = source.ShippingRateId
        };
}
