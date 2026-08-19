using Shared.Application.Domain.Models;
using Shared.Application.Domain.Concerns.Auditable;
using Module.Customer.Domain.Addresses;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Domain.Shipments;

/// <summary>Represents a Shipment.</summary>

// @CAT-10 Invariant: State Pending→Ready→Shipped or →Canceled; Cost >= 0; FinalPrice = Cost + TaxTotal - PromoTotal; ShippedAtUtc set when Shipped
public sealed partial class Shipment : AggregateRoot, IAuditable
{
    #region Properties
    public string TrackingNumber { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentConstant.Defaults.Status;
    #endregion Properties

    #region Timestamps
    public DateTimeOffset? ShippedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; set; }
    #endregion Timestamps

    #region Relationships
    public Guid OrderId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public Guid? AddressId { get; set; }
    #endregion Relationships

    #region Navigation
    public ShippingMethod ShippingMethod { get; set; } = null!;
    public Order? Order { get; set; }
    public Address? Address { get; set; }
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