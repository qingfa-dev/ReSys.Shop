namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

/// <summary>All-nullable parameters for PATCH updates to a shipping method.</summary>
public class ShippingMethodUpdateParameters
{
    public string? Name { get; init; }
    public string? CalculatorType { get; init; }
    public string? Code { get; init; }
    public Guid? TaxCategoryId { get; init; }
    public string? TrackingUrl { get; init; }
    public string? AdminName { get; init; }
    public int? Position { get; init; }
    public bool? AvailableToUsers { get; init; }
    public string? Presentation { get; init; }
}
