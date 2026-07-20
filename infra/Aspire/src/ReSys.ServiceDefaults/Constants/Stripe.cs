namespace ReSys.ServiceDefaults.Constants;

public static class Stripe
{
    public static class Parameters
    {
        public const string ApiKey = "StripeApiKey";
        public const string WebhookSecret = "StripeWebhookSecret";
    }

    public static class EnvironmentVariables
    {
        public const string ApiKey = "STRIPE_API_KEY";
        public const string GatewaySecretKey = "GatewayProviders__stripe__SecretKey";
        public const string GatewayWebhookSecret = "GatewayProviders__stripe__WebhookSecret";
    }

    public static class Cli
    {
        public const string Command = "listen";
        public const string ForwardTo = "--forward-to";
        public const string Events = "--events";
    }

    public const string WebhookRoute = "/api/storefront/webhooks/stripe";

    public const string WebhookEvents =
        "payment_intent.succeeded,payment_intent.payment_failed,payment_intent.canceled," +
        "charge.refunded,charge.dispute.created,payment_intent.processing,payment_intent.requires_action";
}
