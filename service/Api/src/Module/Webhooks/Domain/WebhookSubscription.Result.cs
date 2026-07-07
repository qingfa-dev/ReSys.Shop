namespace Module.Webhooks.Domain;

public static class WebhookSubscriptionResult
{
    public static class Errors
    {
        public static Error NotFound => Error.NotFound(
            code: "WebhookSubscription.NotFound",
            message: "Webhook subscription was not found.");
    }
}
