namespace Module.Webhooks.Features.Admin.Subscriptions.Get.ById;

public static partial class GetWebhookSubscriptionById
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Event { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public bool Active { get; init; }
        public int MaxRetries { get; init; }
        public string? HeadersJson { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
    }
}
