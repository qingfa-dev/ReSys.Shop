// Contract: Failure factories for Stripe gateway validation — pre=responseCode checked, post=Failure.Code matches pattern
namespace Module.Payment.Infrastructure.Gateways.Stripe;

/// <summary>Contains error factory methods for Stripe gateway operations.</summary>
public static class StripeGatewayResult
{
    /// <summary>Error factory methods returning typed Failure instances for Stripe gateway operations.</summary>
    public static class Errors
    {
        /// <summary>PaymentIntent ID is required for capture.</summary>
        public static Error CaptureMissingIntent => Error.Validation(
            code: "Stripe.Capture.MissingIntent",
            description: "PaymentIntent ID required.");

        /// <summary>PaymentIntent ID is required for credit.</summary>
        public static Error CreditMissingIntent => Error.Validation(
            code: "Stripe.Credit.MissingIntent",
            description: "PaymentIntent ID required.");

        /// <summary>PaymentIntent ID is required for cancel.</summary>
        public static Error CancelMissingIntent => Error.Validation(
            code: "Stripe.Cancel.MissingIntent",
            description: "PaymentIntent ID required.");
    }
}
