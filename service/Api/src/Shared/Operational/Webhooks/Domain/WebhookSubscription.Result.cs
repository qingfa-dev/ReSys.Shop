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

        #region Url Validation

        /// <summary>URL must not be empty.</summary>
        public static Error UrlEmpty => Error.Validation(
            code: "Webhooks.Subscription.Url.Empty",
            message: "URL must not be empty.");

        /// <summary>URL must not exceed the maximum length.</summary>
        public static Error SubscriptionUrlTooLong => Error.Validation(
            code: "Webhooks.Subscription.Url.TooLong",
            message: $"URL must not exceed {WebhookSubscriptionConstant.Constraints.Url.MaxLength} characters.");

        /// <summary>URL must be a valid absolute URI.</summary>
        public static Error UrlInvalid => Error.Validation(
            code: "Webhooks.Subscription.Url.Invalid",
            message: "URL must be a valid absolute URI.");

        /// <summary>Only HTTPS URLs are allowed.</summary>
        public static Error UrlScheme => Error.Validation(
            code: "Webhooks.Subscription.Url.Scheme",
            message: "Only HTTPS URLs are allowed.");

        /// <summary>This hostname is not allowed.</summary>
        public static Error UrlBlocked => Error.Validation(
            code: "Webhooks.Subscription.Url.Blocked",
            message: "This hostname is not allowed.");

        /// <summary>Private network addresses are not allowed.</summary>
        public static Error UrlPrivate => Error.Validation(
            code: "Webhooks.Subscription.Url.Private",
            message: "Private network addresses are not allowed.");

        #endregion
    }
}