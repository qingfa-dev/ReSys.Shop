namespace Module.Inventory.Domain.StockMovements;

/// <summary>
/// Defines success messages and error factories for stock movement operations.
/// </summary>
public static class StockMovementResult
{
    /// <summary>
    /// Contains success message templates for stock movement operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Success message for stock movement recording.</summary>
        public static string Recorded => "Stock movement was successfully recorded.";
    }

    /// <summary>
    /// Contains error factory methods for stock movement operations.
    /// </summary>
    public static class Errors
    {
        #region Business
        /// <summary>Error when the stock item for the movement is not found.</summary>
        public static Error StockItemNotFound => Error.NotFound(
            code: "StockMovement.StockItem.NotFound",
            message: "The stock item was not found.");

        /// <summary>Error when movement quantity is zero.</summary>
        public static Error QuantityZero => Error.Validation(
            code: "StockMovement.Quantity.Zero",
            message: "Quantity must not be zero. Use a positive value for received stock or a negative value for shipped stock.");
        #endregion

        #region Validation
        /// <summary>Error when the originator type is not recognised.</summary>
        public static Error InvalidOriginatorType => Error.Validation(
            code: "StockMovement.OriginatorType.Invalid",
            message: "Originator type must be one of: Order, Transfer, Adjustment, Restock.");

        /// <summary>Error when the reason exceeds maximum length.</summary>
        public static Error ReasonTooLong => Error.Validation(
            code: "StockMovement.Reason.TooLong",
            message: $"Reason cannot exceed {StockMovementConstant.Constraints.MaxReasonLength} characters.");

        /// <summary>Error when the action exceeds maximum length.</summary>
        public static Error ActionTooLong => Error.Validation(
            code: "StockMovement.Action.TooLong",
            message: $"Action cannot exceed {StockMovementConstant.Constraints.MaxActionLength} characters.");
        #endregion
    }
}