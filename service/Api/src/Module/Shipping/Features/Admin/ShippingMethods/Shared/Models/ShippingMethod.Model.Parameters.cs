namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

public abstract class ShippingMethodParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? TrackingUrl { get; init; }
    public string? AdminName { get; init; }
    public int Position { get; init; }
    public bool AvailableToUsers { get; init; }
    public string CalculatorType { get; init; } = string.Empty;
    public Guid? TaxCategoryId { get; init; }
    public string? Presentation { get; init; }
}
