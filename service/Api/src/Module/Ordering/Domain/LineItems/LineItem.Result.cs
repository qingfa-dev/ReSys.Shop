namespace Module.Ordering.Domain.LineItems;

/// <summary>
/// Defines success messages and error failures for LineItem operations.
/// </summary>
// Contract: Failure factories with typed error codes — pre=id!=default, post=Failure.Code matches pattern
public static class LineItemResult
{
    /// <summary>
    /// Contains success message factories for LineItem operations.
    /// </summary>
    public static class Success
    {
        public static string Created(Guid id) => $"LineItem with ID '{id}' was successfully created.";
        public static string QuantityUpdated(Guid id) => $"LineItem with ID '{id}' quantity was successfully updated.";
        public static string Recalculated(Guid id) => $"LineItem with ID '{id}' totals were successfully recalculated.";
    }

    /// <summary>
    /// Contains error failure factories for LineItem operations.
    /// </summary>
    public static class Errors
    {
        #region Business
        /// <summary>Returns a not-found failure for the specified line item ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "LineItem.NotFound",
            message: $"LineItem with ID '{id}' was not found.");

        public static Error QuantityExceedsMax => Error.Validation(
            code: "LineItem.Quantity.OutOfRange",
            message: $"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");

        public static Error InvalidPrice => Error.Validation(
            code: "LineItem.Price.Invalid",
            message: "Price must be greater than or equal to zero.");

        public static Error OrderNotFound(Guid id) => Error.NotFound(
            code: "LineItem.Order.NotFound",
            message: $"Order with ID '{id}' was not found.");

        public static Error VariantNotFound(Guid id) => Error.NotFound(
            code: "LineItem.Variant.NotFound",
            message: $"Variant with ID '{id}' was not found.");
        #endregion
    }
}
