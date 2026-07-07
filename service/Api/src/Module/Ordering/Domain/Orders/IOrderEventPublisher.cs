namespace Module.Ordering.Domain.Orders;

public interface IOrderEventPublisher
{
    Task PublishAsync(string eventName, object payload, CancellationToken ct = default);
}
