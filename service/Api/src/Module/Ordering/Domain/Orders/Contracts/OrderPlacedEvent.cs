namespace Module.Ordering.Domain.Orders.Contracts;

public sealed record OrderPlacedEvent(string EventName, object Payload, DateTimeOffset OccurredAtUtc);
