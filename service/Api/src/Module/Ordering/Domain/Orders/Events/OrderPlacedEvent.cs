using BuildingBlocks.Mediators.Events;

namespace Module.Ordering.Domain.Orders.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal Total,
    DateTimeOffset PlacedAtUtc) : DomainEvent(OrderId);
