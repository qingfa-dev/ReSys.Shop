namespace Module.Shipping.Features.Admin.ShippingRates.Shared.Models;

public abstract record ShippingRateParameters : INamedParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public decimal Cost { get; init; }
    public string? DeliveryRange { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public decimal? FreeShippingThreshold { get; init; }
    public Guid ShippingMethodId { get; init; }
}