namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    // EXCEPTION: calculation result with IsFreeShipping — domain-specific, not a shipping method entity
    // EXCEPTION: computed shipping cost — no single domain entity
public sealed record Response
{
    public Guid ShippingMethodId { get; init; }
    public string MethodName { get; init; } = default!;
    public decimal Cost { get; init; }
    public string Currency { get; init; } = default!;
    public bool IsFreeShipping { get; init; }
    public Guid ShippingRateId { get; init; }
}
}