namespace Module.Payment.Domain.RefundReasons;

/// <summary>
/// Provides success messages and error definitions for refund reason operations.
/// </summary>
public static class RefundReasonResult
{
    /// <summary>
    /// Contains success message templates for refund reason lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a created refund reason.</summary>
        public static string Created(Guid id) => $"RefundReason with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for an activated refund reason.</summary>
        public static string Activated(Guid id) => $"RefundReason with ID '{id}' was successfully activated.";
        /// <summary>Returns a success message for a deactivated refund reason.</summary>
        public static string Deactivated(Guid id) => $"RefundReason with ID '{id}' was successfully deactivated.";
    }

    /// <summary>
    /// Contains error definitions for refund reason validation.
    /// </summary>
    public static class Errors
    {
        /// <summary>Error indicating the refund reason was not found.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "RefundReason.NotFound",
            description: $"RefundReason with ID '{id}' was not found.");

        /// <summary>Error indicating the refund reason name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "RefundReason.NameRequired",
            description: "Refund reason name is required.");

        /// <summary>Error indicating a refund reason with this code already exists.</summary>
        public static Error CodeDuplicate => Error.Conflict(
            code: "RefundReason.CodeDuplicate",
            description: "A refund reason with this code already exists.");
    }
}