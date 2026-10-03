using Shared.Application.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecordOrderShipmentState;

public sealed record RecordOrderShipmentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    public ShipmentState FulfillmentState { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
}
