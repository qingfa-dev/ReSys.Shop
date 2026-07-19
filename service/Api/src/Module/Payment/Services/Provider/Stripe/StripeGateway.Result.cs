namespace Module.Payment.Services.Provider.Stripe;

// Contract: Error factories for Stripe gateway operations — pre=responseCode!=null
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

        public static Error PurchaseNotSucceeded(string status) => Error.BadRequest(
            "Stripe.Purchase.NotSucceeded",
            $"Purchase status: {status}");

        public static Error AuthorizeNotRequiresCapture(string status) => Error.BadRequest(
            "Stripe.Authorize.NotRequiresCapture",
            $"Authorize status: {status}");

        public static Error GatewayError(string code, string message) => Error.BadRequest(
            $"Stripe.{code}",
            message);
    }
}