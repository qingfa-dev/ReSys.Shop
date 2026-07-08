namespace Module.Payment.Domain.Payments;

/// <summary>
/// Provides success messages and error definitions for payment domain operations.
/// </summary>
public static class PaymentResult
{
    /// <summary>
    /// Contains success message templates for payment lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a created payment.</summary>
        public static string Created(string number) => $"Payment '{number}' was successfully created.";
        /// <summary>Returns a success message for a processed payment.</summary>
        public static string Processed(string number) => $"Payment '{number}' was successfully processed.";
        /// <summary>Returns a success message for a pended payment.</summary>
        public static string Pended(string number) => $"Payment '{number}' was successfully pended.";
        /// <summary>Returns a success message for a completed payment.</summary>
        public static string Completed(string number) => $"Payment '{number}' was successfully completed.";
        /// <summary>Returns a success message for a voided payment.</summary>
        public static string Voided(string number) => $"Payment '{number}' was successfully voided.";
        /// <summary>Returns a failure notification message.</summary>
        public static string Failed(string number) => $"Payment '{number}' was marked as failed.";
        /// <summary>Returns a success message for a captured amount on a payment.</summary>
        public static string Captured(string number, decimal amount) => $"Payment '{number}' was captured for {amount}.";
        /// <summary>Returns a success message for a credited amount on a payment.</summary>
        public static string Credited(string number, decimal amount) => $"Payment '{number}' was credited for {amount}.";
        /// <summary>Returns a success message for a refunded amount on a payment.</summary>
        public static string Refunded(string number, decimal amount) => $"Payment '{number}' was refunded for {amount}.";
    }

    /// <summary>
    /// Contains error definitions for payment validation and business rule violations.
    /// </summary>
    public static class Failure
    {
        #region Validation
        /// <summary>Error indicating the payment amount must be greater than zero.</summary>
        public static Error AmountMustBePositive => Error.Validation(
            code: "Payment.Amount.Positive",
            message: "Payment amount must be greater than zero.");

        /// <summary>Error indicating an invalid payment state transition was attempted.</summary>
        public static Error InvalidStateTransition(PaymentState from, PaymentState to) => Error.Validation(
            code: "Payment.State.InvalidTransition",
            message: $"Cannot transition payment from '{from}' to '{to}'.");
        #endregion Validation

        #region Business
        /// <summary>Error indicating the payment was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Payment.NotFound",
            message: "Payment was not found.");

        /// <summary>Error indicating the payment has already been completed.</summary>
        public static Error AlreadyCompleted => Error.Conflict(
            code: "Payment.AlreadyCompleted",
            message: "Payment has already been completed.");

        /// <summary>Error indicating the payment has already been voided.</summary>
        public static Error AlreadyVoided => Error.Conflict(
            code: "Payment.AlreadyVoided",
            message: "Payment has already been voided.");

        /// <summary>Error indicating the payment has already been marked as failed.</summary>
        public static Error AlreadyFailed => Error.Conflict(
            code: "Payment.AlreadyFailed",
            message: "Payment has already been marked as failed.");

        /// <summary>Error indicating the capture amount exceeds the authorized amount.</summary>
        public static Error AmountExceedsAuthorized => Error.Validation(
            code: "Payment.Amount.ExceedsAuthorized",
            message: "Capture amount exceeds the authorized amount.");

        /// <summary>Error indicating the payment method is inactive and cannot process.</summary>
        public static Error PaymentMethodInactive => Error.Conflict(
            code: "Payment.PaymentMethod.Inactive",
            message: "Cannot process payment because the payment method is inactive.");

        /// <summary>Payment source is required but was not provided.</summary>
        public static Error ProcessingSourceRequired => Error.Validation(
            code: "Payment.Processing.SourceRequired",
            message: "Payment source is required but was not provided.");

        /// <summary>Payment is already being processed.</summary>
        public static Error ProcessingAlreadyProcessing => Error.Conflict(
            code: "Payment.Processing.AlreadyProcessing",
            message: "Payment is already being processed.");

        /// <summary>Payment is not in a completed state and cannot be credited.</summary>
        public static Error CreditNotAllowed => Error.Conflict(
            code: "Payment.Credit.NotAllowed",
            message: "Payment is not in a completed state and cannot be credited.");
        #endregion Business

        #region Gateway Response Errors (dynamic message)
        /// <summary>Gateway capture action failed.</summary>
        public static Error CaptureFailed(string message) => Error.BadRequest(
            code: "Payment.Capture.Failed",
            message: message);

        /// <summary>Gateway void action failed.</summary>
        public static Error VoidFailed(string message) => Error.BadRequest(
            code: "Payment.Void.Failed",
            message: message);

        /// <summary>Gateway cancel action failed.</summary>
        public static Error CancelFailed(string message) => Error.BadRequest(
            code: "Payment.Cancel.Failed",
            message: message);

        /// <summary>Gateway credit action failed.</summary>
        public static Error CreditFailed(string message) => Error.BadRequest(
            code: "Payment.Credit.Failed",
            message: message);

        /// <summary>Gateway action returned a failure response.</summary>
        public static Error GatewayError(string message) => Error.BadRequest(
            code: "Payment.Gateway.Error",
            message: message);
        #endregion Gateway Response Errors
    }
}