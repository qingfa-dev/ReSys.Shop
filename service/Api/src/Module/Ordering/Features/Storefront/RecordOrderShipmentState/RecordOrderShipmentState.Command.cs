using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecordOrderShipmentState;

public sealed record RecordOrderShipmentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    public OrderFulfillmentState FulfillmentState { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
}
