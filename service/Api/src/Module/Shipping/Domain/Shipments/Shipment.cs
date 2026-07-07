using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;
using Module.Identity.Domain.Addresses;
using Module.Ordering.Domain.Orders;
using Module.Inventory.Domain.StockLocations;

using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Domain.Shipments;
/// <summary>Represents a Shipment.</summary>

// @CAT-10 Invariant: State Pending→Ready→Shipped or →Canceled; Cost >= 0; FinalPrice = Cost + TaxTotal - PromoTotal; ShippedAtUtc set when Shipped
public sealed partial class Shipment : Entity, IAuditable
{
    #region Properties
    public string Number { get; set; } = string.Empty;
    public ShipmentState State { get; set; } = ShipmentConstant.Defaults.State;
    public string? Tracking { get; set; }
    public decimal Cost { get; set; }
    public decimal DiscountedCost { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal ItemCost { get; set; }
    public decimal AdditionalTaxTotal { get; set; }
    public decimal IncludedTaxTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal PromoTotal { get; set; }
    #endregion Properties

    #region Timestamps
    public DateTimeOffset? ShippedAtUtc { get; set; }
    #endregion Timestamps

    #region Relationships
    public Guid OrderId { get; set; }
    public Guid StockLocationId { get; set; }
    public Guid? ShippingMethodId { get; set; }
    public Guid? AddressId { get; set; }
    #endregion Relationships

    #region Navigation
    public Order Order { get; set; } = null!;
    public StockLocation? StockLocation { get; set; }
    public Address? Address { get; set; }
    public ICollection<ShippingRate> ShippingRates { get; set; } = [];
    #endregion Navigation

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal Shipment() { }
    #endregion Constructor
}