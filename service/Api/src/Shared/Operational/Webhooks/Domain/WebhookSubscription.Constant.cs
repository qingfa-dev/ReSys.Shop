namespace Shared.Operational.Webhooks.Domain;

public static class WebhookSubscriptionConstant
{
    public static class Defaults
    {
        public const bool Active = true;
        public const int MaxRetries = 3;
    }

    public static class Constraints
    {
        public static class Event
        {
            public const int MaxLength = 100;
        }

        public static class Url
        {
            public const int MaxLength = 2048;
        }

        public static class SecretHash
        {
            public const int MaxLength = 256;
        }

        public static class HeadersJson
        {
            public const string ColumnType = "jsonb";
        }
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(WebhookSubscription.Event),
            nameof(WebhookSubscription.Url),
            nameof(WebhookSubscription.CreatedAtUtc),
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(WebhookSubscription.Event),
            nameof(WebhookSubscription.Active),
            nameof(WebhookSubscription.CreatedAtUtc),
            nameof(WebhookSubscription.ModifiedAtUtc),
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(WebhookSubscription.Event),
            nameof(WebhookSubscription.Active),
            nameof(WebhookSubscription.MaxRetries),
            nameof(WebhookSubscription.CreatedAtUtc),
        ];
    }
}