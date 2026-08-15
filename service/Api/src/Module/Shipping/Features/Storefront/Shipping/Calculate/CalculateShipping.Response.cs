using Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public sealed record Response : ShippingCalculationParameters;
}
