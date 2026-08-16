namespace Module.Ordering.Domain.Adjustments;

/// <summary>
/// Defines success messages and error failures for Adjustment operations.
/// </summary>
// Contract: Failure factories for state/existence violations
public static class AdjustmentResult
{
    /// <summary>
    /// Contains success message factories for Adjustment operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Adjustment was created and applied.</summary>
        public static string Created(Guid id) => $"Adjustment with ID '{id}' was successfully created.";
        /// <summary>Adjustment was updated.</summary>
        public static string Updated(Guid id) => $"Adjustment with ID '{id}' was successfully updated.";
        /// <summary>Adjustment was closed and frozen for recalculation.</summary>
        public static string Closed => "Adjustment was successfully closed.";
        /// <summary>Adjustment was reopened for modification.</summary>
        public static string Opened => "Adjustment was successfully opened.";
    }

    /// <summary>
    /// Contains error failure factories for Adjustment operations.
    /// </summary>
    public static class Errors
    {
        #region Existence
        /// <summary>Returns a not-found failure for the specified adjustment ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Adjustment.NotFound",
            message: $"Adjustment with ID '{id}' was not found.");
        #endregion

        #region Validation
        /// <summary>Adjustment label is required.</summary>
        public static Error LabelRequired => Error.Validation(
            code: "Adjustment.Label.Required",
            message: "Adjustment label is required.");

        /// <summary>Adjustment label exceeds maximum length.</summary>
        public static Error LabelTooLong => Error.Validation(
            code: "Adjustment.Label.TooLong",
            message: $"Adjustment label cannot exceed {AdjustmentConstant.Constraints.MaxLabelLength} characters.");

        /// <summary>Adjustment amount is required.</summary>
        public static Error AmountRequired => Error.Validation(
            code: "Adjustment.Amount.Required",
            message: "Adjustment amount is required.");

        /// <summary>Adjustment amount must be greater than or equal to zero.</summary>
        public static Error InvalidAmount => Error.Validation(
            code: "Adjustment.Amount.Invalid",
            message: "Adjustment amount must be greater than or equal to zero.");

        /// <summary>Adjustment type is required.</summary>
        public static Error TypeRequired => Error.Validation(
            code: "Adjustment.Type.Required",
            message: "Adjustment type is required.");

        /// <summary>Adjustment type exceeds maximum length.</summary>
        public static Error TypeTooLong => Error.Validation(
            code: "Adjustment.Type.TooLong",
            message: $"Adjustment type cannot exceed {AdjustmentConstant.Constraints.MaxTypeStrings} characters.");

        /// <summary>Adjustment state is required.</summary>
        public static Error StateRequired => Error.Validation(
            code: "Adjustment.State.Required",
            message: "Adjustment state is required.");

        /// <summary>Adjustment state exceeds maximum length.</summary>
        public static Error StateTooLong => Error.Validation(
            code: "Adjustment.State.TooLong",
            message: $"Adjustment state cannot exceed {AdjustmentConstant.Constraints.MaxStateLength} characters.");

        /// <summary>Adjustment state is not a known lifecycle state.</summary>
        public static Error StateInvalid => Error.Validation(
            code: "Adjustment.State.Invalid",
            message: "Adjustment state must be either 'open' or 'closed'.");

        /// <summary>Adjustable entity reference is required.</summary>
        public static Error AdjustableRequired => Error.Validation(
            code: "Adjustment.Adjustable.Required",
            message: "Adjustable entity reference is required.");

        /// <summary>Source reference is required.</summary>
        public static Error SourceRequired => Error.Validation(
            code: "Adjustment.Source.Required",
            message: "Source reference is required.");

        /// <summary>Adjustable type must be one of: Order, LineItem, Shipment.</summary>
        public static Error InvalidAdjustableType => Error.Validation(
            code: "Adjustment.AdjustableType.Invalid",
            message: $"Adjustable type must be one of: Order, LineItem, Shipment.");

        /// <summary>Source type must be one of: Shipping.</summary>
        public static Error InvalidSourceType => Error.Validation(
            code: "Adjustment.SourceType.Invalid",
            message: $"Source type must be one of: Shipping.");
        #endregion

        #region State
        /// <summary>Adjustment is already closed.</summary>
        public static Error AlreadyClosed => Error.Conflict(
            code: "Adjustment.AlreadyClosed",
            message: "Adjustment is already closed.");

        /// <summary>Adjustment is already open.</summary>
        public static Error AlreadyOpen => Error.Conflict(
            code: "Adjustment.AlreadyOpen",
            message: "Adjustment is already open.");
        #endregion

        #region Misc
        /// <summary>Invalid adjustment action specified.</summary>
        public static Error ActionInvalid => Error.Validation(
            code: "Adjustment.Action.Invalid",
            message: "Invalid adjustment action.");
        #endregion
    }
}