using Shared.Application.Domain.Models;

namespace Shared.Operational.Webhooks.Domain;

public sealed class WebhookDelivery : Entity
{
    public Guid SubscriptionId { get; set; }
    public string Event { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextRetryAtUtc { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
}
