namespace Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Models;


public abstract record ShippingCalculationParameters
{
    public Guid ShippingMethodId { get; init; }
    public string MethodName { get; init; } = default!;
    public decimal Cost { get; init; }
    public string Currency { get; init; } = default!;
    public bool IsFreeShipping { get; init; }
    public Guid ShippingRateId { get; init; }
}
