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
        public static string Created(Guid id) => $"Adjustment with ID '{id}' was successfully created.";
        public static string Updated(Guid id) => $"Adjustment with ID '{id}' was successfully updated.";
        public static string Closed => "Adjustment was successfully closed.";
        public static string Opened => "Adjustment was successfully opened.";
    }

    /// <summary>
    /// Contains error failure factories for Adjustment operations.
    /// </summary>
    public static class Errors
    {
        /// <summary>Returns a not-found failure for the specified adjustment ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Adjustment.NotFound",
            message: $"Adjustment with ID '{id}' was not found.");

        public static Error InvalidAmount => Error.Validation(
            code: "Adjustment.Amount.Invalid",
            message: "Adjustment amount must be greater than or equal to zero.");

        public static Error AdjustableRequired => Error.Validation(
            code: "Adjustment.Adjustable.Required",
            message: "Adjustable entity reference is required.");

        public static Error SourceRequired => Error.Validation(
            code: "Adjustment.Source.Required",
            message: "Source reference is required.");

        public static Error InvalidAdjustableType => Error.Validation(
            code: "Adjustment.AdjustableType.Invalid",
            message: $"Adjustable type must be one of: Order, LineItem, Shipment.");

        public static Error InvalidSourceType => Error.Validation(
            code: "Adjustment.SourceType.Invalid",
            message: $"Source type must be one of: PromotionAction, TaxRate.");

        public static Error AlreadyClosed => Error.Conflict(
            code: "Adjustment.AlreadyClosed",
            message: "Adjustment is already closed.");

        public static Error AlreadyOpen => Error.Conflict(
            code: "Adjustment.AlreadyOpen",
            message: "Adjustment is already open.");

        /// <summary>Invalid adjustment action specified.</summary>
        public static Error ActionInvalid => Error.Validation(
            code: "Adjustment.Action.Invalid",
            message: "Invalid adjustment action.");
    }
}
