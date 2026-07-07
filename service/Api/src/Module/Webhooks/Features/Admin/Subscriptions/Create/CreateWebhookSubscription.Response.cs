namespace Module.Webhooks.Features.Admin.Subscriptions.Create;

public static partial class CreateWebhookSubscription
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Event { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public bool Active { get; init; }
        public int MaxRetries { get; init; }
    }
}
