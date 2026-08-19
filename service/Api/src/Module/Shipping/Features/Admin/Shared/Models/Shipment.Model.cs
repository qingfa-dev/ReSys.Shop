using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shared.Models;

#region Parameters
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
#endregion

#region Request
public record ShipmentUpsertRequest : ShipmentStatusParameters;

public abstract record ShipmentStatusParameters
{
    public ShipmentStatus Status { get; init; }
    public string? TrackingNumber { get; init; }
}
#endregion

#region Response
public record ShipmentDetailResponse : ShipmentParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public record ShipmentListItemResponse : ShipmentDetailResponse;

#endregion