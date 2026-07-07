using BuildingBlocks.Mediators.Events;

namespace Module.Shipping.Domain.Shipments.Events;

public static class ShipmentEvent
{
    public static class Lifecycle
    {
        public sealed record Shipped(
            Guid ShipmentId,
            Guid OrderId,
            string TrackingNumber,
            string? Carrier,
            DateTimeOffset ShippedAtUtc,
            Guid CustomerId,
            string CustomerEmail) : DomainEvent(ShipmentId);
    }
}
