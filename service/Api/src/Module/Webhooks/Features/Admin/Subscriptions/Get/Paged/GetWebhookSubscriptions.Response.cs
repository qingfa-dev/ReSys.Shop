namespace Module.Webhooks.Features.Admin.Subscriptions.Get.Paged;

public static partial class GetWebhookSubscriptions
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Event { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public bool Active { get; init; }
        public int MaxRetries { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
