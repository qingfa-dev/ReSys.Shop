namespace Module.Billing.Domain.WebhookEvents;

public static class WebhookEventResult
{
    public static class Success
    {
        public static string Processed(string id) => $"Webhook event {id} was processed.";
    }

    public static class Errors
    {
        public static Error NotFound => Error.NotFound(
            code: "WebhookEvent.NotFound",
            message: "Webhook event was not found.");

        public static Error StripeEventIdRequired => Error.Validation(
            code: "WebhookEvent.StripeEventId.Required",
            message: "Stripe event id is required.");

        public static Error StripeEventIdTooLong => Error.Validation(
            code: "WebhookEvent.StripeEventId.TooLong",
            message: $"Stripe event id cannot exceed {WebhookEventConstant.Constraints.MaxStripeEventIdLength} characters.");

        public static Error TypeRequired => Error.Validation(
            code: "WebhookEvent.Type.Required",
            message: "Webhook event type is required.");

        public static Error TypeTooLong => Error.Validation(
            code: "WebhookEvent.Type.TooLong",
            message: $"Webhook event type cannot exceed {WebhookEventConstant.Constraints.MaxTypeLength} characters.");
    }
}