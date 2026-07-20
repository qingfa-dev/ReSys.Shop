namespace Module.Shipping.Features.Storefront.Shared.Models;

public abstract record ShippingMethodParameters
{
    public string MethodName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Cost { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int EstimatedDaysMin { get; init; }
    public int EstimatedDaysMax { get; init; }
    public bool IsActive { get; init; } = true;
}