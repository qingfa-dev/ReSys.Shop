using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Models;

public abstract record ShipmentUpdateParameters
{
    public ShipmentStatus Status { get; init; }
    public string? TrackingNumber { get; init; }
}

public record ShipmentUpdateRequest : ShipmentUpdateParameters;
