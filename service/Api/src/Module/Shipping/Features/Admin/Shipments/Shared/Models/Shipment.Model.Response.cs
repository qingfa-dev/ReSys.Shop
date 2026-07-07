namespace Module.Shipping.Features.Admin.Shipments.Shared.Models;

/// <summary>Detail response for a shipment, including audit fields.</summary>
public class ShipmentDetailResponse : ShipmentParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

/// <summary>List item response for a shipment.</summary>
public class ShipmentListItemResponse : ShipmentDetailResponse;
