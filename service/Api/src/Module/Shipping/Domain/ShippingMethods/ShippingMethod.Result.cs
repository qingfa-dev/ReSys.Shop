namespace Module.Shipping.Domain.ShippingMethods;

/// <summary>
/// Provides success and error result factories for shipping method operations.
/// </summary>
// Result: Centralized success/error messages for shipping method domain operations
public static class ShippingMethodResult
{
    /// <summary>
    /// Contains success message factories for shipping method operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for method creation.</summary>
        public static string Created => "Shipping method was successfully created.";
        /// <summary>Returns a success message for method update.</summary>
        public static string Updated => "Shipping method was successfully updated.";
        /// <summary>Returns a success message for method deletion.</summary>
        public static string Deleted => "Shipping method was successfully deleted.";
    }

    /// <summary>
    /// Contains error factory methods for shipping method failures.
    /// </summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Returns a validation error indicating method name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "ShippingMethod.Name.Required",
            message: "Shipping method name is required.");

        /// <summary>Returns a validation error indicating method name is too long.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "ShippingMethod.Name.TooLong",
            message: $"Shipping method name cannot exceed {ShippingMethodConstant.Constraints.MaxNameLength} characters.");

        /// <summary>Returns a validation error indicating code is too long.</summary>
        public static Error CodeTooLong => Error.Validation(
            code: "ShippingMethod.Code.TooLong",
            message: $"Shipping method code cannot exceed {ShippingMethodConstant.Constraints.MaxCodeLength} characters.");

        /// <summary>Returns a conflict error indicating the code is already in use.</summary>
        public static Error CodeDuplicate => Error.Conflict(
            code: "ShippingMethod.Code.Duplicate",
            message: "A shipping method with the same code already exists.");

        /// <summary>Returns a validation error indicating the tracking URL is too long.</summary>
        public static Error InvalidTrackingUrl => Error.Validation(
            code: "ShippingMethod.TrackingUrl.Invalid",
            message: $"Shipping method tracking URL cannot exceed {ShippingMethodConstant.Constraints.MaxTrackingUrlLength} characters.");

        /// <summary>Returns a validation error indicating admin name is too long.</summary>
        public static Error AdminNameTooLong => Error.Validation(
            code: "ShippingMethod.AdminName.TooLong",
            message: $"Shipping method admin name cannot exceed {ShippingMethodConstant.Constraints.MaxAdminNameLength} characters.");

        /// <summary>Returns a validation error indicating calculator type is required.</summary>
        public static Error CalculatorRequired => Error.Validation(
            code: "ShippingMethod.Calculator.Required",
            message: "Shipping method calculator type is required.");

        /// <summary>Returns a validation error indicating calculator type is too long.</summary>
        public static Error CalculatorTooLong => Error.Validation(
            code: "ShippingMethod.Calculator.TooLong",
            message: $"Shipping method calculator type cannot exceed {ShippingMethodConstant.Constraints.MaxCalculatorTypeLength} characters.");

        /// <summary>Returns a validation error indicating presentation is too long.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "ShippingMethod.Presentation.TooLong",
            message: $"Shipping method presentation cannot exceed {ShippingMethodConstant.Constraints.MaxPresentationLength} characters.");

        /// <summary>Returns a validation error indicating the ID is required.</summary>
        public static Error IdRequired => Error.Validation(
            code: "ShippingMethod.Id.Required",
            message: "Shipping method ID is required.");
        #endregion Validation

        #region Business
        /// <summary>Returns a not-found error for the shipping method.</summary>
        public static Error NotFound => Error.NotFound(
            code: "ShippingMethod.NotFound",
            message: "Shipping method was not found.");

        /// <summary>Returns a not-found error when no rates are available for the method.</summary>
        public static Error NoRateAvailable => Error.NotFound(
            code: "ShippingMethod.NoRateAvailable",
            message: "No shipping rate is available for this method.");
        #endregion Business
    }

    /// <summary>
    /// Contains precondition-failure error factories for shipping method operations.
    /// </summary>
    public static class Failure
    {
        /// <summary>Returns a conflict error when deactivating a method with active orders.</summary>
        public static Error HasActiveOrders => Error.Conflict(
            code: "ShippingMethod.Deactivate.HasActiveOrders",
            message: "Cannot deactivate a shipping method that has active orders.");

        /// <summary>Returns a conflict error when deleting a method with associated rates.</summary>
        public static Error HasAssociatedRates => Error.Conflict(
            code: "ShippingMethod.Delete.HasAssociatedRates",
            message: "Cannot delete a shipping method with associated shipping rates.");
    }
}