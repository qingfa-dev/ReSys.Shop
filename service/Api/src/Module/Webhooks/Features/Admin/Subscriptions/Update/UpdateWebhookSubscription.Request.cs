namespace Module.Webhooks.Features.Admin.Subscriptions.Update;

public static partial class UpdateWebhookSubscription
{
    public sealed class Request
    {
        public string? Url { get; init; }
        public bool? Active { get; init; }
        public int? MaxRetries { get; init; }
        public string? HeadersJson { get; init; }
    }
}
