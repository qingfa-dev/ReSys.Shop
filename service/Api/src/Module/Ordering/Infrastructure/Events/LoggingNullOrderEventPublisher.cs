using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders.Contracts;

namespace Module.Ordering.Infrastructure.Events;

public sealed class LoggingNullOrderEventPublisher(ILogger<LoggingNullOrderEventPublisher> logger) : IOrderEventPublisher
{
    private int _count;

    public Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
    {
        var n = Interlocked.Increment(ref _count);
        if (n == 1)
        {
            logger.LogWarning(
                "LoggingNullOrderEventPublisher is dropping events. Configure a real publisher before production cutover. First event: {EventName}",
                eventName);
        }
        return Task.CompletedTask;
    }
}
