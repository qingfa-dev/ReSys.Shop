using Shared.Application.Domain.Models;

namespace Module.Billing.Domain.WebhookEvents;

public sealed partial class WebhookEvent : Entity
{
    public string StripeEventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public WebhookEventState State { get; set; } = WebhookEventState.Pending;
    public int AttemptCount { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
