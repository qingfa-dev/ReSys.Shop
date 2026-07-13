namespace Module.Catalog.Domain.Products.Variants;

public static class VariantResult
{
    public static class Success
    {
        /// <summary>Returns a success message for variant creation.</summary>
        public static string Created(Guid id) => $"Variant with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for variant update.</summary>
        public static string Updated(Guid id) => $"Variant with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for variant deletion.</summary>
        public static string Deleted(Guid id) => $"Variant with ID '{id}' was successfully deleted.";
        /// <summary>Variant logistics fields were successfully updated.</summary>
        public static string LogisticsUpdated => "Variant logistics fields were successfully updated.";
        /// <summary>Variant was successfully discontinued.</summary>
        public static string Discontinued => "Variant was successfully discontinued.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>SKU is required.</summary>
        public static Error SkuRequired => Error.Validation(
            code: "Variant.Sku.Required",
            message: "SKU is required.");

        /// <summary>SKU exceeds the maximum length.</summary>
        public static Error SkuTooLong => Error.Validation(
            code: "Variant.Sku.TooLong",
            message: $"SKU cannot exceed {VariantConstant.Constraints.SkuMaxLength} characters.");

        /// <summary>Price must be greater than or equal to the minimum value.</summary>
        public static Error InvalidPrice => Error.Validation(
            code: "Variant.Price.Invalid",
            message: $"Price must be greater than or equal to {VariantConstant.Constraints.MinPrice}.");

        /// <summary>Dimensions must be greater than or equal to the minimum value.</summary>
        public static Error InvalidDimension => Error.Validation(
            code: "Variant.Dimension.Invalid",
            message: $"Dimensions must be greater than or equal to {VariantConstant.Constraints.Dimensions.MinValue}.");

        /// <summary>Position must be at least the minimum value.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "Variant.Position.Invalid",
            message: $"Position must be at least {VariantConstant.Constraints.MinPosition}.");

        /// <summary>Weight must be greater than or equal to the minimum value.</summary>
        public static Error InvalidWeight => Error.Validation(
            code: "Variant.Weight.Invalid",
            message: $"Weight must be greater than or equal to {VariantConstant.Constraints.Weight.MinValue}.");

        /// <summary>Weight unit is not a valid value.</summary>
        public static Error InvalidWeightUnit => Error.Validation(
            code: "Variant.WeightUnit.Invalid",
            message: "Weight unit is invalid.");

        /// <summary>Dimensions unit is not a valid value.</summary>
        public static Error InvalidDimensionsUnit => Error.Validation(
            code: "Variant.DimensionsUnit.Invalid",
            message: "Dimensions unit is invalid.");

        /// <summary>Cost price must be greater than or equal to zero.</summary>
        public static Error InvalidCostPrice => Error.Validation(
            code: "Variant.CostPrice.Invalid",
            message: "Cost price must be greater than or equal to zero.");

        /// <summary>Cost currency is not in the allowed list.</summary>
        public static Error InvalidCostCurrency => Error.Validation(
            code: "Variant.CostCurrency.Invalid",
            message: $"Cost currency is invalid. Must be one of: {string.Join(", ", VariantConstant.Constraints.Price.AllowedCurrencies)}.");
        #endregion

        #region Business
        /// <summary>Variant was not found by ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Variant.NotFound",
            message: $"Variant with ID '{id}' was not found.");

        /// <summary>Variant was not found.</summary>
        public static Error GenericNotFound => Error.NotFound(
            code: "Variant.NotFound",
            message: "The requested product variant was not found.");

        /// <summary>Duplicate SKU detected.</summary>
        public static Error DuplicateSku => Error.Conflict(
            code: "Variant.DuplicateSku",
            message: "A variant with the same SKU already exists.");

        /// <summary>SKU already exists.</summary>
        public static Error SkuAlreadyExists(string sku) => Error.Conflict(
            code: "Variant.SkuAlreadyExists",
            message: $"A variant with SKU '{sku}' already exists.");

        /// <summary>Variant is already deleted.</summary>
        public static Error AlreadyDeleted => Error.Conflict(
            code: "Variant.AlreadyDeleted",
            message: "Variant is already deleted.");

        /// <summary>Master variant cannot have specific option values.</summary>
        public static Error MasterCannotHaveOptions => Error.Conflict(
            code: "Variant.MasterCannotHaveOptions",
            message: "Master variant cannot have specific option values.");

        /// <summary>Variant is already discontinued.</summary>
        public static Error AlreadyDiscontinued => Error.Conflict(
            code: "Variant.AlreadyDiscontinued",
            message: "Variant is already discontinued.");

        /// <summary>Duplicate barcode detected.</summary>
        public static Error BarcodeDuplicate => Error.Conflict(
            code: "Variant.Barcode.Duplicate",
            message: "A variant with the same barcode already exists.");

        /// <summary>HS code exceeds the maximum length.</summary>
        public static Error HsCodeTooLong => Error.Validation(
            code: "Variant.HsCode.TooLong",
            message: $"HS code cannot exceed {VariantConstant.Constraints.HsCodeMaxLength} characters.");

        /// <summary>Barcode exceeds the maximum length.</summary>
        public static Error BarcodeTooLong => Error.Validation(
            code: "Variant.Barcode.TooLong",
            message: $"Barcode cannot exceed {VariantConstant.Constraints.BarcodeMaxLength} characters.");

        /// <summary>Variant has no default price set.</summary>
        public static Error NoDefaultPrice => Error.Validation(
            code: "Variant.Price.NoDefault",
            message: "Variant has no default price set.");

        /// <summary>Variant is not purchasable in its current state.</summary>
        public static Error NotPurchasable => Error.Conflict(
            code: "Variant.NotPurchasable",
            message: "Variant is not purchasable in its current state.");

        /// <summary>Master variant cannot be discontinued directly.</summary>
        public static Error MasterCannotBeDiscontinued => Error.Conflict(
            code: "Variant.MasterCannotBeDiscontinued",
            message: "Master variant cannot be discontinued directly.");
        #endregion
    }
}