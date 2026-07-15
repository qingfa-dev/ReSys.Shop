namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    // EXCEPTION: calculation result with IsFreeShipping — domain-specific, not a shipping method entity
    public sealed record Response(Guid ShippingMethodId, string MethodName, decimal Cost, string Currency, bool IsFreeShipping);
}