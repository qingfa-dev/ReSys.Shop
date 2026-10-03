namespace Module.Catalog.Domain.Products.Classifications;

public static class ClassificationResult
{
    public static class Success
    {
        /// <summary>Product classification was successfully created.</summary>
        public static string Created => "Product classification was successfully created.";
        /// <summary>Product classification was successfully deleted.</summary>
        public static string Deleted => "Product classification was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Product ID is required.</summary>
        public static Error ProductIdRequired => Error.Validation(
            code: "Classification.ProductId.Required",
            message: "Product ID is required.");

        /// <summary>Taxon ID is required.</summary>
        public static Error TaxonIdRequired => Error.Validation(
            code: "Classification.TaxonId.Required",
            message: "Taxon ID is required.");

        /// <summary>Position must be at least the minimum value.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "Classification.Position.Invalid",
            message: $"Position must be at least {ClassificationConstant.Constraints.MinPosition}.");

        /// <summary>Taxon IDs must be provided.</summary>
        public static Error TaxonIdsRequired => Error.Validation(
            code: "Classification.TaxonIds.Required",
            message: "Taxon IDs must be provided.");

        /// <summary>At least one taxon ID must be provided.</summary>
        public static Error TaxonIdsEmpty => Error.Validation(
            code: "Classification.TaxonIds.Empty",
            message: "At least one taxon ID must be provided.");
        #endregion

        #region Business
        /// <summary>Classification was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Classification.NotFound",
            message: "Classification was not found.");

        /// <summary>This product is already classified under this taxon.</summary>
        public static Error AlreadyExists => Error.Conflict(
            code: "Classification.AlreadyExists",
            message: "This product is already classified under this taxon.");
        #endregion
    }
}