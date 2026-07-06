namespace Module.Inventory.Domain.StockLocations.StockItems;

/// <summary>
/// Defines success messages and error factories for stock item operations.
/// </summary>
public static class StockItemResult
{
    /// <summary>
    /// Contains success message templates for stock item operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Success message for stock item creation.</summary>
        public static string Created(Guid id) => $"StockItem with ID '{id}' was successfully created.";
        /// <summary>Success message for stock item count adjustment.</summary>
        public static string Adjusted => "StockItem count-on-hand was successfully adjusted.";
        /// <summary>Success message for stock item restock.</summary>
        public static string Restocked => "StockItem was successfully restocked.";
        /// <summary>Success message for stock item pick.</summary>
        public static string Picked => "StockItem was successfully picked.";
        public static string BulkAdjusted => "StockItems were successfully bulk adjusted.";
    }

    /// <summary>
    /// Contains error factory methods for stock item operations.
    /// </summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Error when stock location ID is required but missing.</summary>
        public static Error StockLocationIdRequired => Error.Validation(
            code: "StockItem.StockLocationIdRequired",
            message: "Stock location ID is required.");

        /// <summary>Error when variant ID is required but missing.</summary>
        public static Error VariantIdRequired => Error.Validation(
            code: "StockItem.VariantIdRequired",
            message: "Variant ID is required.");

        /// <summary>Error when count-on-hand would become negative.</summary>
        public static Error NegativeCountOnHand => Error.Validation(
            code: "StockItem.CountOnHand.Negative",
            message: "Count-on-hand cannot be negative.");
        #endregion

        #region Business
        /// <summary>Error when stock item is not found.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "StockItem.NotFound",
            message: $"StockItem with ID '{id}' was not found.");

        /// <summary>Error when there is insufficient stock to fulfil a request.</summary>
        public static Error InsufficientStock => Error.Validation(
            code: "StockItem.InsufficientStock",
            message: "Insufficient stock to fulfill the requested quantity.");

        /// <summary>Error when the stock location is not found.</summary>
        public static Error LocationNotFound(Guid id) => Error.NotFound(
            code: "StockItem.LocationNotFound",
            message: $"StockLocation with ID '{id}' was not found.");

        /// <summary>Error when the variant is not found.</summary>
        public static Error VariantNotFound(Guid id) => Error.NotFound(
            code: "StockItem.VariantNotFound",
            message: $"Variant with ID '{id}' was not found.");

        /// <summary>Error when a stock item already exists for the variant and location combination.</summary>
        public static Error AlreadyExists(Guid variantId, Guid locationId) => Error.Conflict(
            code: "StockItem.AlreadyExists",
            message: $"A StockItem already exists for Variant '{variantId}' at StockLocation '{locationId}'.");
        #endregion
    }
}
