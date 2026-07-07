using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Services;

public interface IWebhookDispatcher
{
    Task<Result> PublishAsync(string eventName, object payload, CancellationToken ct = default);

    Task<Result<WebhookDelivery>> DeliverAsync(WebhookSubscription subscription, WebhookDelivery delivery, CancellationToken ct = default);
}
