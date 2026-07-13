namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public sealed record Response(Guid ShippingMethodId, string MethodName, decimal Cost, string Currency, bool IsFreeShipping);
}