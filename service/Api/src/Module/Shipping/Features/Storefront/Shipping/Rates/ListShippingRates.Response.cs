namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    // EXCEPTION: rate-specific fields (FinalPrice, DeliveryRange, etc.) not in ShippingMethodDetailResponse
    // EXCEPTION: shipping rate DTO — no single domain entity for this computed rate
public sealed record Response
{
    public Guid Id { get; init; }
    public Guid ShippingMethodId { get; init; }
    public string Name { get; init; } = default!;
    public decimal Cost { get; init; }
    public decimal FinalPrice { get; init; }
    public string? DeliveryRange { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public decimal? FreeShippingThreshold { get; init; }
}
}