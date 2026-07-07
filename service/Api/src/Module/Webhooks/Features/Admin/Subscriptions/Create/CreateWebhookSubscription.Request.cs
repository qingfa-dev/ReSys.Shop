namespace Module.Webhooks.Features.Admin.Subscriptions.Create;

public static partial class CreateWebhookSubscription
{
    public sealed class Request
    {
        public string Event { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string Secret { get; init; } = string.Empty;
        public string? HeadersJson { get; init; }
        public int MaxRetries { get; init; } = 3;
    }
}
