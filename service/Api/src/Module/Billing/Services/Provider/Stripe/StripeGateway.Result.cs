namespace Module.Billing.Services.Provider.Stripe;

/// <summary>Error factories for Stripe gateway payment operations.</summary>
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

        public static Result<PaymentGatewayResponse> TransientGatewayError(string code, string message) =>
            Result<PaymentGatewayResponse>.Failure(
                Error.Unexpected($"Stripe.Transient.{code}", message));

        public static Result<PaymentGatewayResponse> PaymentMethodRequired(string? message) =>
            Result<PaymentGatewayResponse>.Failure(
                Error.Validation(
                    "Stripe.PaymentMethod.Required",
                    $"Payment method was declined or requires re-entry: {message ?? "unknown"}"));

        public static Result<PaymentGatewayResponse> AmountExceedsMaximum =>
            Result<PaymentGatewayResponse>.Failure(
                Error.Validation(
                    "Stripe.Amount.ExceedsMaximum",
                    "Payment amount exceeds the maximum supported value."));
    }
}