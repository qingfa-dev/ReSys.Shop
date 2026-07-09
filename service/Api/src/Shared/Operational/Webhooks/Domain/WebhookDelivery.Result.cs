namespace Shared.Operational.Webhooks.Domain;

public static class WebhookDeliveryResult
{
    public static class Success
    {
        public const string Created = "Webhook delivery created successfully.";
        public const string Dispatched = "Webhook delivery dispatched.";
        public const string Delivered = "Webhook delivered successfully.";
    }

    public static class Failure
    {
        public static Error NotFound => Error.NotFound(
            code: "WebhookDelivery.NotFound",
            message: "The specified webhook delivery was not found.");

        public static Error SubscriptionRequired => Error.Validation(
            code: "WebhookDelivery.Subscription.Required",
            message: "Subscription ID is required.");

        public static Error EventRequired => Error.Validation(
            code: "WebhookDelivery.Event.Required",
            message: "Event name is required.");

        public static Error PayloadRequired => Error.Validation(
            code: "WebhookDelivery.Payload.Required",
            message: "Payload JSON is required.");

        public static Error InvalidStatus(string status) => Error.Validation(
            code: "WebhookDelivery.Status.Invalid",
            message: $"Cannot transition from status '{status}'.");
    }
}