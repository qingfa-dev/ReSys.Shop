namespace Module.Shipping.Features.Admin.Shipments.Shared.Models;

/// <summary>Abstract base class for shipment parameters.</summary>
public abstract class ShipmentParameters
{
    public string Number { get; init; } = string.Empty;
    public string? Tracking { get; init; }
    public decimal Cost { get; init; }
    public decimal DiscountedCost { get; init; }
    public decimal FinalPrice { get; init; }
    public decimal ItemCost { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal PromoTotal { get; init; }
    public Guid OrderId { get; init; }
    public Guid StockLocationId { get; init; }
    public Guid? ShippingMethodId { get; init; }
    public Guid? AddressId { get; init; }
}
