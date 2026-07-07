using Microsoft.Extensions.Logging;

using Module.Ordering.Domain.Orders;

using Shared.Operational.Webhooks.Services;

namespace Module.Ordering.Infrastructure.Events;

public sealed class WebhookOrderEventPublisher : IOrderEventPublisher
{
    private readonly IWebhookDispatcher _dispatcher;
    private readonly ILogger<WebhookOrderEventPublisher> _logger;

    public WebhookOrderEventPublisher(IWebhookDispatcher dispatcher, ILogger<WebhookOrderEventPublisher> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
    {
        var result = await _dispatcher.PublishAsync(eventName, payload, ct);
        if (result.IsFailure)
        {
            _logger.LogWarning("Webhook publish failed for {Event}: {Errors}",
                eventName, string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}
