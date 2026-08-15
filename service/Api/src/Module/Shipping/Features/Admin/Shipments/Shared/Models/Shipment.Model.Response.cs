namespace Module.Shipping.Features.Admin.Shipments.Shared.Models;

public record ShipmentDetailResponse : ShipmentParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public record ShipmentListItemResponse : ShipmentDetailResponse;

public record ShipmentListResponse
{
    public IReadOnlyList<ShipmentListItemResponse> Items { get; init; } = [];
}
