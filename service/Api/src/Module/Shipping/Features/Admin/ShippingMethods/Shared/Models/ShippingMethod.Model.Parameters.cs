namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

/// <summary>Abstract base class for shipping method parameters.</summary>
public abstract class ShippingMethodParameters
{
    public string Name { get; init; } = string.Empty;
    public string CalculatorType { get; init; } = string.Empty;
    public string? Code { get; init; }
    public Guid? TaxCategoryId { get; init; }
    public string? TrackingUrl { get; init; }
    public string? AdminName { get; init; }
    public int Position { get; init; }
    public bool AvailableToUsers { get; init; } = true;
}
