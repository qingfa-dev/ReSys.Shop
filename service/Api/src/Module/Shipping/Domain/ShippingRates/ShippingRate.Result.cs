namespace Module.Shipping.Domain.ShippingRates;

/// <summary>
/// Provides success and error result factories for shipping rate operations.
/// </summary>
// Result: Centralized success/error messages for shipping rate domain operations
public static class ShippingRateResult
{
    /// <summary>
    /// Contains success message factories for shipping rate operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for rate creation.</summary>
        public static string Created(Guid id) => $"ShippingRate with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for rate selection.</summary>
        public static string Selected(Guid id) => $"ShippingRate with ID '{id}' was successfully selected.";
        /// <summary>Returns a success message for rate unselection.</summary>
        public static string Unselected(Guid id) => $"ShippingRate with ID '{id}' was successfully unselected.";
    }

    /// <summary>
    /// Contains error factory methods for shipping rate failures.
    /// </summary>
    public static class Errors
    {
        /// <summary>Creates a not-found error for the given rate ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "ShippingRate.NotFound",
            description: $"ShippingRate with ID '{id}' was not found.");

        /// <summary>Returns a conflict error indicating the rate is already selected.</summary>
        public static Error AlreadySelected => Error.Conflict(
            code: "ShippingRate.AlreadySelected",
            description: "Shipping rate is already selected.");

        /// <summary>Returns a conflict error indicating the rate is not currently selected.</summary>
        public static Error NotSelected => Error.Conflict(
            code: "ShippingRate.NotSelected",
            description: "Shipping rate is not currently selected.");

        /// <summary>Returns a validation error indicating cost must be greater than zero.</summary>
        public static Error CostRequired => Error.Validation(
            code: "ShippingRate.CostRequired",
            description: "Shipping rate cost must be greater than zero.");

        /// <summary>Returns a validation error indicating name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "ShippingRate.Name.Required",
            description: "Shipping rate name is required.");

        /// <summary>Returns a validation error indicating name exceeds maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "ShippingRate.Name.TooLong",
            description: $"Shipping rate name cannot exceed {ShippingRateConstant.Constraints.MaxNameLength} characters.");

        /// <summary>Returns a validation error when MinWeight exceeds MaxWeight.</summary>
        public static Error MinWeightExceedsMaxWeight => Error.Validation(
            code: "ShippingRate.MinWeight.ExceedsMax",
            description: "Minimum weight cannot exceed maximum weight.");

        /// <summary>Returns a validation error when weight is negative.</summary>
        public static Error WeightNegative => Error.Validation(
            code: "ShippingRate.Weight.Negative",
            description: "Weight values must not be negative.");
    }
}