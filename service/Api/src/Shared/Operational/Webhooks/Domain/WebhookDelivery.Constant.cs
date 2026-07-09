namespace Shared.Operational.Webhooks.Domain;

/// <summary>Contains default values, constraints, and retry configuration for WebhookDelivery entities.</summary>
public static class WebhookDeliveryConstant
{
    /// <summary>Default configuration values for delivery behavior.</summary>
    public static class Defaults
    {
        public const int MaxRetries = 3;
        /// <summary>Base delay between retries in seconds (300s = 5 minutes).</summary>
        public const int RetryDelayBaseSeconds = 300;
        /// <summary>Exponential multiplier for retry backoff.</summary>
        public const int RetryDelayExponentBase = 5;
    }

    /// <summary>Max-length constraints for delivery properties.</summary>
    public static class Constraints
    {
        public static class Event
        {
            public const int MaxLength = 100;
        }

        public static class PayloadJson
        {
            public const string ColumnType = "jsonb";
        }

        public static class LastError
        {
            public const int MaxLength = 2048;
        }
    }
}
