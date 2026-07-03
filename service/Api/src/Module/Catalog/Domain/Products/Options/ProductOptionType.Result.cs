namespace Module.Catalog.Domain.Products.Options;

public static class ProductOptionTypeResult
{
    public static class Success
    {
        /// <summary>Product option type association was successfully created.</summary>
        public static string Created => "Product option type association was successfully created.";
        /// <summary>Product option type association was successfully deleted.</summary>
        public static string Deleted => "Product option type association was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Product ID is required.</summary>
        public static Error  ProductIdRequired => Error.Validation(
            code: "ProductOptionType.ProductId.Required",
            message: "Product ID is required.");

        /// <summary>Option type ID is required.</summary>
        public static Error  OptionTypeIdRequired => Error.Validation(
            code: "ProductOptionType.OptionTypeId.Required",
            message: "Option type ID is required.");

        /// <summary>Position must be at least the minimum value.</summary>
        public static Error  InvalidPosition => Error.Validation(
            code: "ProductOptionType.Position.Invalid",
            message: $"Position must be at least {ProductOptionTypeConstant.Constraints.MinPosition}.");

        /// <summary>Option type IDs must be provided.</summary>
        public static Error  OptionTypeIdsRequired => Error.Validation(
            code: "ProductOptionType.OptionTypeIds.Required",
            message: "Option type IDs must be provided.");

        /// <summary>At least one option type ID must be provided.</summary>
        public static Error  OptionTypeIdsEmpty => Error.Validation(
            code: "ProductOptionType.OptionTypeIds.Empty",
            message: "At least one option type ID must be provided.");
        #endregion

        #region Business
        /// <summary>Product option type association was not found.</summary>
        public static Error  NotFound => Error.NotFound(
            code: "ProductOptionType.NotFound",
            message: "Product option type association was not found.");

        /// <summary>This product is already associated with this option type.</summary>
        public static Error  AlreadyExists => Error.Conflict(
            code: "ProductOptionType.AlreadyExists",
            message: "This product is already associated with this option type.");
        #endregion
    }
}