using System.Threading.Channels;
using Module.Ordering.Domain.Orders.Contracts;

namespace Module.Ordering.Infrastructure.Events;

public sealed class InProcessOrderEventPublisher : IOrderEventPublisher
{
    private readonly Channel<OrderPlacedEvent> _channel = Channel.CreateUnbounded<OrderPlacedEvent>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ChannelReader<OrderPlacedEvent> Reader => _channel.Reader;

    public async Task PublishAsync(string eventName, object payload, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(new OrderPlacedEvent(eventName, payload, DateTimeOffset.UtcNow), ct);
    }
}
