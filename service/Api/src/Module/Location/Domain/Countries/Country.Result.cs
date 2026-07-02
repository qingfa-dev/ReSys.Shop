namespace Module.Location.Domain.Countries;

/// <summary>Contains success messages and domain error definitions for Country operations.</summary>
public static class CountryResult
{
    // Success
    public static class Success
    {
        public const string Summary = "Country details retrieved successfully.";
        public const string Created = "Country created successfully.";
        public const string Updated = "Country updated successfully.";
        public const string GetList = "List of countries retrieved successfully.";
        public const string GetSelectList = "Country select list retrieved successfully.";

        // Import Success
        public const string Imported = "Country imported successfully.";
        public const string ImportedWithErrors = "Country import completed with some errors.";
    }

    // Errors
    /// <summary>Domain error definitions for Country operations.</summary>
    public static class Errors
    {
        // Check: Predefined failures for common country-related error scenarios
        /// <summary>Country not found by the specified identifier.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Country.NotFound",
            message: "The specified country was not found.");

        // Check: Predefined failure for inactive countries
        /// <summary>Country is inactive and cannot be used.</summary>
        public static Error Inactive => Error.Validation(
            code: "Country.Inactive",
            message: "The selected country is not active.");

        // Check: Predefined failure for cannot delete with states
        /// <summary>Cannot delete a country that has existing states.</summary>
        public static Error CannotDeleteWithStates => Error.Validation(
            code: "Country.CannotDelete",
            message: "Cannot delete country with existing states.");

        // Check: Predefined failure for has active states
        /// <summary>Cannot deactivate a country that still has active states.</summary>
        public static Error HasActiveStates => Error.Validation(
            code: "Country.HasActiveStates",
            message: "Cannot deactivate a country that has active states.");

        #region Validations

        // Validate: Name rules
        /// <summary>Country name is required and cannot be empty.</summary>
        public static Error NameRequired => Error.Validation(
            code: "Country.Name.Required",
            message: "Country name cannot be empty.");

        /// <summary>Country name exceeds the maximum allowed length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "Country.Name.TooLong",
            message:
            $"Country name cannot exceed {CountryConstant.Constraints.MaxNameLength} characters.");

        // Validate: ISO Code rules
        /// <summary>ISO code is required and cannot be empty.</summary>
        public static Error IsoCodeRequired => Error.Validation(
            code: "Country.IsoCode.Required",
            message: "ISO code cannot be empty.");

        /// <summary>ISO code exceeds the maximum allowed length.</summary>
        public static Error IsoCodeTooLong => Error.Validation(
            code: "Country.IsoCode.TooLong",
            message:
            $"ISO code cannot exceed {CountryConstant.Constraints.MaxIsoCodeLength} characters.");

        /// <summary>ISO code already exists in the system.</summary>
        public static Error IsoCodeDuplicate => Error.Conflict(
            code: "Country.IsoCode.Duplicate",
            message: "A country with the same ISO code already exists.");

        // Validate: Iso3Code rules
        /// <summary>ISO 3-letter code is required and cannot be empty.</summary>
        public static Error Iso3CodeRequired => Error.Validation(
            code: "Country.Iso3Code.Required",
            message: "ISO 3-letter code cannot be empty.");

        /// <summary>ISO 3-letter code exceeds the maximum allowed length.</summary>
        public static Error Iso3CodeTooLong => Error.Validation(
            code: "Country.Iso3Code.TooLong",
            message:
            $"ISO 3-letter code cannot exceed {CountryConstant.Constraints.MaxIso3CodeLength} characters.");

        // Validate: IsoName rules
        /// <summary>ISO country name is required and cannot be empty.</summary>
        public static Error IsoNameRequired => Error.Validation(
            code: "Country.IsoName.Required",
            message: "ISO country name cannot be empty.");

        /// <summary>ISO country name exceeds the maximum allowed length.</summary>
        public static Error IsoNameTooLong => Error.Validation(
            code: "Country.IsoName.TooLong",
            message:
            $"ISO country name cannot exceed {CountryConstant.Constraints.MaxIsoNameLength} characters.");

        // Validate: Calling Code rules
        /// <summary>Calling code exceeds the maximum allowed length.</summary>
        public static Error CallingCodeTooLong => Error.Validation(
            code: "Country.CallingCode.TooLong",
            message:
            $"Calling code cannot exceed {CountryConstant.Constraints.MaxCallingCodeLength} characters.");

        /// <summary>Country ID is required for the operation.</summary>
        public static Error IdRequired => Error.Validation(
            code: "Country.Id.Required",
            message: "Country ID is required.");

        #endregion

    }
}
