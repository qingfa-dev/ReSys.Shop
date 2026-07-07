namespace Module.Payment.Domain.PaymentCaptureEvents;

/// <summary>
/// Provides success messages and error definitions for payment capture event operations.
/// </summary>
public static class PaymentCaptureEventResult
{
    /// <summary>
    /// Contains success message templates for payment capture event lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a recorded payment capture event.</summary>
        public static string Recorded => "Payment capture event recorded successfully.";
    }

    /// <summary>
    /// Contains error definitions for payment capture event validation.
    /// </summary>
    public static class Errors
    {
        /// <summary>Error indicating the payment capture event was not found.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "PaymentCaptureEvent.NotFound",
            description: $"Payment capture event with ID '{id}' was not found.");

        /// <summary>Error indicating the capture amount must be greater than zero.</summary>
        public static Error InvalidAmount => Error.Validation(
            code: "PaymentCaptureEvent.Amount.Invalid",
            description: "Amount must be greater than zero.");
    }
}