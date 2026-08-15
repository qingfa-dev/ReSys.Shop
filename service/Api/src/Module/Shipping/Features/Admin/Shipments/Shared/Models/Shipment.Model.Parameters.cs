using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Models;

public abstract record ShipmentParameters
{
    public Guid OrderId { get; init; }
    public Guid ShippingMethodId { get; init; }
    public string? TrackingNumber { get; init; }
    public ShipmentStatus Status { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; init; }
}
