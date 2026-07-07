namespace Module.Shipping.Features.Admin.MethodRates.Shared.Models;

/// <summary>Abstract base class for method rate (shipping rate) parameters.</summary>
public abstract class MethodRateParameters
{
    public string Name { get; init; } = string.Empty;
    public decimal Cost { get; init; }
    public decimal FinalPrice { get; init; }
    public string? DeliveryRange { get; init; }
    public Guid ShippingMethodId { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public decimal? FreeShippingThreshold { get; init; }
}
