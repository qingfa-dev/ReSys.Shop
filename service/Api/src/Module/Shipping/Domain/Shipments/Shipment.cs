using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Shipping.Domain.Shipments;

public sealed partial class Shipment : Entity, IAuditable
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public string? TrackingNumber { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public DateTimeOffset? ShippedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; set; }
    #endregion Properties

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
