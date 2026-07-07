using BuildingBlocks.Mediators.Events;

namespace Module.Ordering.Domain.Orders.Events;

public sealed record OrderCanceledEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    DateTimeOffset CanceledAtUtc,
    string? CanceledBy) : DomainEvent(OrderId);
