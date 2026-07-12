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
        /// <summary>Line item was added to the order.</summary>
        public static string Created(Guid id) => $"LineItem with ID '{id}' was successfully created.";
        /// <summary>Line item quantity was updated.</summary>
        public static string QuantityUpdated(Guid id) => $"LineItem with ID '{id}' quantity was successfully updated.";
        /// <summary>Line item totals were recalculated.</summary>
        public static string Recalculated(Guid id) => $"LineItem with ID '{id}' totals were successfully recalculated.";
        /// <summary>Line item was removed from the order.</summary>
        public static string Removed(Guid id) => $"LineItem with ID '{id}' was removed.";
        /// <summary>Line item was updated by admin.</summary>
        public static string Updated(Guid id) => $"LineItem with ID '{id}' was updated.";
    }

    /// <summary>
    /// Contains error failure factories for LineItem operations.
    /// </summary>
    public static class Errors
    {
        #region Existence
        /// <summary>Returns a not-found failure for the specified line item ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "LineItem.NotFound",
            message: $"LineItem with ID '{id}' was not found.");

        public static Error OrderNotFound(Guid id) => Error.NotFound(
            code: "LineItem.Order.NotFound",
            message: $"Order with ID '{id}' was not found.");

        public static Error VariantNotFound(Guid id) => Error.NotFound(
            code: "LineItem.Variant.NotFound",
            message: $"Variant with ID '{id}' was not found.");
        #endregion

        #region Validation
        /// <summary>Line item ID is required.</summary>
        public static Error IdRequired => Error.Validation(
            code: "LineItem.Id.Required",
            message: "Line item ID is required.");

        /// <summary>Quantity exceeds the maximum allowed per line item.</summary>
        public static Error QuantityExceedsMax => Error.Validation(
            code: "LineItem.Quantity.OutOfRange",
            message: $"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");

        /// <summary>Price must be greater than or equal to zero.</summary>
        public static Error InvalidPrice => Error.Validation(
            code: "LineItem.Price.Invalid",
            message: "Price must be greater than or equal to zero.");
        #endregion
    }
}
