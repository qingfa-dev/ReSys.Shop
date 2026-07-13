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
        /// <summary>Adjustment amount must be greater than or equal to zero.</summary>
        public static Error InvalidAmount => Error.Validation(
            code: "Adjustment.Amount.Invalid",
            message: "Adjustment amount must be greater than or equal to zero.");

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