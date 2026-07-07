namespace Module.Webhooks.Features.Admin.Subscriptions.Test;

public static partial class TestWebhookSubscription
{
    public sealed class Response
    {
        public Guid DeliveryId { get; init; }
        public string Status { get; init; } = string.Empty;
        public int AttemptCount { get; init; }
        public string? LastError { get; init; }
    }
}
