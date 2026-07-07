using BuildingBlocks.Mediators.Events;

namespace Module.Ordering.Domain.Orders.Events;

public sealed record OrderResumedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    DateTimeOffset ResumedAtUtc) : DomainEvent(OrderId);
