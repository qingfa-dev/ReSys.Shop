namespace Module.Payment.Domain.PaymentMethods;

/// <summary>
/// Provides success messages and error definitions for payment method domain operations.
/// </summary>
public static class PaymentMethodResult
{
    /// <summary>
    /// Contains success message templates for payment method lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a created payment method.</summary>
        public static string Created(string name) => $"Payment method '{name}' was successfully created.";
        /// <summary>Returns a success message for an activated payment method.</summary>
        public static string Activated(string name) => $"Payment method '{name}' was successfully activated.";
        /// <summary>Returns a success message for a deactivated payment method.</summary>
        public static string Deactivated(string name) => $"Payment method '{name}' was successfully deactivated.";
        /// <summary>Returns a success message for an updated payment method.</summary>
        public static string Updated(string name) => $"Payment method '{name}' was successfully updated.";
    }

    /// <summary>
    /// Contains error definitions for payment method validation and business rule violations.
    /// </summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Error indicating the payment method name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "PaymentMethod.Name.Required",
            message: "Payment method name is required.");

        /// <summary>Error indicating the payment method name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "PaymentMethod.Name.TooLong",
            message: $"Payment method name cannot exceed {PaymentMethodConstant.Constraints.MaxNameLength} characters.");

        /// <summary>Error indicating the payment method code exceeds the maximum length.</summary>
        public static Error CodeTooLong => Error.Validation(
            code: "PaymentMethod.Code.TooLong",
            message: $"Payment method code cannot exceed {PaymentMethodConstant.Constraints.MaxCodeLength} characters.");

        /// <summary>Error indicating the provider type is required.</summary>
        public static Error ProviderTypeRequired => Error.Validation(
            code: "PaymentMethod.ProviderType.Required",
            message: "Payment method provider type is required.");

        /// <summary>Error indicating the provider type exceeds the maximum length.</summary>
        public static Error ProviderTypeTooLong => Error.Validation(
            code: "PaymentMethod.ProviderType.TooLong",
            message: $"Payment method provider type cannot exceed {PaymentMethodConstant.Constraints.MaxProviderTypeLength} characters.");

        /// <summary>Error indicating the description exceeds the maximum length.</summary>
        public static Error DescriptionTooLong => Error.Validation(
            code: "PaymentMethod.Description.TooLong",
            message: $"Payment method description cannot exceed {PaymentMethodConstant.Constraints.MaxDescriptionLength} characters.");
        #endregion Validation

        #region Business
        /// <summary>Error indicating the payment method was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "PaymentMethod.NotFound",
            message: "Payment method was not found.");

        /// <summary>Error indicating a payment method with the same code already exists.</summary>
        public static Error CodeDuplicate => Error.Conflict(
            code: "PaymentMethod.Code.Duplicate",
            message: "A payment method with the same code already exists.");

        /// <summary>Error indicating the payment method is already active.</summary>
        public static Error AlreadyActive => Error.Conflict(
            code: "PaymentMethod.AlreadyActive",
            message: "Payment method is already active.");

        /// <summary>Error indicating the payment method is already inactive.</summary>
        public static Error AlreadyInactive => Error.Conflict(
            code: "PaymentMethod.AlreadyInactive",
            message: "Payment method is already inactive.");
        #endregion Business
    }
}