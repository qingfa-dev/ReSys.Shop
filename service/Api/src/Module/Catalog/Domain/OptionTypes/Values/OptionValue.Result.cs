namespace Module.Catalog.Domain.OptionTypes.Values;

public static class OptionValueResult
{
    public static class Success
    {
        /// <summary>Returns a success message for option value creation.</summary>
        public static string Created(Guid id) => $"Option value with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for option value update.</summary>
        public static string Updated(Guid id) => $"Option value with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for option value deletion.</summary>
        public static string Deleted(Guid id) => $"Option value with ID '{id}' was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Option type ID is required.</summary>
        public static Error OptionTypeIdRequired => Error.Validation(
            code: "OptionValue.OptionTypeId.Required",
            message: "Option type ID is required.");

        /// <summary>Name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "OptionValue.Name.Required",
            message: "Name is required.");

        /// <summary>Name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "OptionValue.Name.TooLong",
            message: $"Name exceeds maximum length of {OptionValueConstant.Constraints.NameMaxLength} characters.");

        /// <summary>Presentation is required.</summary>
        public static Error PresentationRequired => Error.Validation(
            code: "OptionValue.Presentation.Required",
            message: "Presentation is required.");

        /// <summary>Presentation exceeds the maximum length.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "OptionValue.Presentation.TooLong",
            message: $"Presentation exceeds maximum length of {OptionValueConstant.Constraints.PresentationMaxLength} characters.");

        /// <summary>Position must be greater than or equal to the minimum.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "OptionValue.Position.Invalid",
            message: $"Position must be greater than or equal to {OptionValueConstant.Constraints.MinPosition}.");
        #endregion

        #region Business
        /// <summary>Option value was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "OptionValue.NotFound",
            message: "Option value was not found.");

        /// <summary>Duplicate names found in the synchronization request.</summary>
        public static Error NameDuplicated => Error.Validation(
            code: "OptionValue.NameDuplicated",
            message: "Duplicate names found in the synchronization request.");

        /// <summary>Option value already exists for this option type.</summary>
        public static Error NameAlreadyExists => Error.Conflict(
            code: "OptionValue.NameAlreadyExists",
            message: "Option value already exists for this option type.");
        #endregion
    }
}