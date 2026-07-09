namespace Shared.Operational.Webhooks.Domain;

public static class WebhookSubscriptionErrors
{
    public static class Success
    {
        public const string Created = "Webhook subscription created successfully.";
        public const string Updated = "Webhook subscription updated successfully.";
        public const string Deleted = "Webhook subscription deleted successfully.";
        public const string TestDispatched = "Webhook test delivery dispatched.";
    }

    public static class Failure
    {
        public static Error NotFound => Error.NotFound(
            code: "WebhookSubscription.NotFound",
            message: "The specified webhook subscription was not found.");

        public static Error EventRequired => Error.Validation(
            code: "WebhookSubscription.Event.Required",
            message: "Event name is required.");

        public static Error EventTooLong => Error.Validation(
            code: "WebhookSubscription.Event.TooLong",
            message: $"Event name cannot exceed {WebhookSubscriptionConstant.Constraints.Event.MaxLength} characters.");

        public static Error UrlRequired => Error.Validation(
            code: "WebhookSubscription.Url.Required",
            message: "URL is required.");

        public static Error UrlTooLong => Error.Validation(
            code: "WebhookSubscription.Url.TooLong",
            message: $"URL cannot exceed {WebhookSubscriptionConstant.Constraints.Url.MaxLength} characters.");

        public static Error SecretHashRequired => Error.Validation(
            code: "WebhookSubscription.SecretHash.Required",
            message: "Secret hash is required.");
    }
}