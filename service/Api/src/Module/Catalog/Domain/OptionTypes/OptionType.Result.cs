namespace Module.Catalog.Domain.OptionTypes;

public static class OptionTypeResult
{
    public static class Success
    {
        /// <summary>Option type was successfully created.</summary>
        public static string Created=> $"Option type was successfully created.";
        /// <summary>Option type was successfully updated.</summary>
        public static string Updated => $"Option type was successfully updated.";
        /// <summary>Option type was successfully deleted.</summary>
        public static string Deleted => $"Option type was successfully deleted.";
    }

    public static class Failure
    {
        #region Validation
        /// <summary>Option type name is required.</summary>
        public static Error  NameRequired => Error.Validation(
            code: "OptionType.Name.Required",
            message: "Option type name is required.");

        /// <summary>Option type name exceeds the maximum length.</summary>
        public static Error  NameTooLong => Error.Validation(
            code: "OptionType.Name.TooLong",
            message: $"Option type name cannot exceed {OptionTypeConstant.Constraints.NameMaxLength} characters.");

        /// <summary>Option type presentation is required.</summary>
        public static Error  PresentationRequired => Error.Validation(
            code: "OptionType.Presentation.Required",
            message: "Option type presentation is required.");

        /// <summary>Option type presentation exceeds the maximum length.</summary>
        public static Error  PresentationTooLong => Error.Validation(
            code: "OptionType.Presentation.TooLong",
            message: $"Option type presentation cannot exceed {OptionTypeConstant.Constraints.PresentationMaxLength} characters.");

        /// <summary>Position must be greater than or equal to the minimum.</summary>
        public static Error  InvalidPosition => Error.Validation(
            code: "OptionType.Position.Invalid",
            message: $"Position must be greater than or equal to {OptionTypeConstant.Constraints.MinPosition}.");
        #endregion

        #region Business
        /// <summary>Option type was not found.</summary>
        public static Error  NotFound => Error.NotFound(
            code: "OptionType.NotFound",
            message: $"Option type was not found.");

        /// <summary>An option type with the same name already exists.</summary>
        public static Error  DuplicateName => Error.Conflict(
            code: "OptionType.DuplicateName",
            message: "An option type with the same name already exists.");

        /// <summary>Cannot delete an option type that has associated values.</summary>
        public static Error  CannotDeleteWithValues => Error.Conflict(
            code: "OptionType.CannotDeleteWithValues",
            message: "Cannot delete an option type that has associated values.");
        #endregion
    }
}