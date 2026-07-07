using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Infrastructure.Events;

public sealed class NullOrderEventPublisher : IOrderEventPublisher
{
    public Task PublishAsync(string eventName, object payload, CancellationToken ct = default) => Task.CompletedTask;
}
