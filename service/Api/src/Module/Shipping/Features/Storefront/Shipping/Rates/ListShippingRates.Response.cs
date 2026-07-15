namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    // EXCEPTION: rate-specific fields (FinalPrice, DeliveryRange, etc.) not in ShippingMethodDetailResponse
    public sealed record Response(Guid Id, Guid ShippingMethodId, string Name, decimal Cost, decimal FinalPrice, string? DeliveryRange, decimal? MinWeight, decimal? MaxWeight, decimal? FreeShippingThreshold);
}