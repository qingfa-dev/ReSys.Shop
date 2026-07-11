using Module.Payment.Services.Gateways;

namespace Module.Payment.Services.Gateways.Stripe;

public static class StripeGatewayResult
{
    public static class Errors
    {
        public static Error CaptureMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CaptureMissingIntent,
            "PaymentIntent ID required.");

        public static Error CreditMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CreditMissingIntent,
            "PaymentIntent ID required.");

        public static Error CancelMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CancelMissingIntent,
            "PaymentIntent ID required.");
    }
}
