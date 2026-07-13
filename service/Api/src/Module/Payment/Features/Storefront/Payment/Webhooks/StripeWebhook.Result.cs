// Contract: Failure factories for Stripe webhook validation — pre=signature/payload checked, post=Failure.Code matches pattern
namespace Module.Payment.Features.Storefront.Payment.Webhooks;

/// <summary>Contains success messages and error factory methods for Stripe Webhook operations.</summary>
public static class StripeWebhookResult
{
    /// <summary>Error factory methods returning typed Failure instances for Stripe webhook operations.</summary>
    public static class Errors
    {
        /// <summary>Stripe webhook signature validation failed.</summary>
        public static Error InvalidSignature => Error.Unauthorized(
            code: "Stripe.Webhook.InvalidSignature",
            message: "Invalid Stripe webhook signature.");

        /// <summary>Stripe webhook payload could not be parsed.</summary>
        public static Error InvalidPayload => Error.BadRequest(
            code: "Stripe.Webhook.InvalidPayload",
            message: "Unable to parse Stripe webhook payload.");
    }
}