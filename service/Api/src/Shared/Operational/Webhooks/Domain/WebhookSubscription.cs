using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Shared.Operational.Webhooks.Domain;

public sealed class WebhookSubscription : Entity, IAuditable
{
    public string Event { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SecretHash { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public string? HeadersJson { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}
