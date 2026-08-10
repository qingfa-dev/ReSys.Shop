namespace Module.Catalog.Domain.Variants.Options;

public static class OptionValueVariantResult
{
    public static class Success
    {
        /// <summary>Variant option value association was successfully created.</summary>
        public static string Created => "Variant option value association was successfully created.";
        /// <summary>Variant option value association was successfully deleted.</summary>
        public static string Deleted => "Variant option value association was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Variant ID is required.</summary>
        public static Error VariantIdRequired => Error.Validation(
            code: "OptionValueVariant.VariantId.Required",
            message: "Variant ID is required.");

        /// <summary>Option value ID is required.</summary>
        public static Error OptionValueIdRequired => Error.Validation(
            code: "OptionValueVariant.OptionValueId.Required",
            message: "Option value ID is required.");

        /// <summary>Option value IDs must be provided.</summary>
        public static Error OptionValueIdsRequired => Error.Validation(
            code: "OptionValueVariant.OptionValueIds.Required",
            message: "Option value IDs must be provided.");

        /// <summary>At least one option value ID must be provided.</summary>
        public static Error OptionValueIdsEmpty => Error.Validation(
            code: "OptionValueVariant.OptionValueIds.Empty",
            message: "At least one option value ID must be provided.");
        #endregion

        #region Business
        /// <summary>Variant option value association was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "OptionValueVariant.NotFound",
            message: "Variant option value association was not found.");

        /// <summary>This variant is already associated with this option value.</summary>
        public static Error AlreadyExists => Error.Conflict(
            code: "OptionValueVariant.AlreadyExists",
            message: "This variant is already associated with this option value.");

        /// <summary>A variant can only have one value per option type.</summary>
        public static Error MultipleValuesPerOptionType => Error.Conflict(
            code: "OptionValueVariant.MultipleValuesPerOptionType",
            message: "A variant can only have one value per option type.");
        #endregion
    }
}