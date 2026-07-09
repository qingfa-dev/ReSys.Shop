namespace Module.Ordering.Domain.Orders.Contracts;

public interface IOrderEventPublisher
{
    Task PublishAsync(string eventName, object payload, CancellationToken ct = default);
}
