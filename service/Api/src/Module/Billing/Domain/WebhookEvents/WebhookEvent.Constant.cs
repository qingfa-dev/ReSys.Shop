namespace Module.Billing.Domain.WebhookEvents;

public static class WebhookEventConstant
{
    public static class Constraints
    {
        public const int MaxStripeEventIdLength = 200;
        public const int MaxTypeLength = 100;
    }

    public static class Defaults
    {
        public const int AttemptCount = 0;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(WebhookEvent.StripeEventId),
            nameof(WebhookEvent.Type)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(WebhookEvent.CreatedAtUtc),
            nameof(WebhookEvent.ProcessedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(WebhookEvent.State),
            nameof(WebhookEvent.AttemptCount)
        ];
    }
}