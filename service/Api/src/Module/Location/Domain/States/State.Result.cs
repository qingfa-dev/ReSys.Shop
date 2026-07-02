namespace Module.Location.Domain.States;

public static class StateResult
{
    // Success
    public static class Success
    {
        public const string Summary = "State details retrieved successfully.";
        public const string Created = "State created successfully.";
        public const string Updated = "State updated successfully.";
        public const string GetList = "List of states retrieved successfully.";
        public const string GetSelectList = "State select list retrieved successfully.";
    }

    // Errors
    public static class Errors
    {
        // Check: Predefined failures for common state-related error scenarios
        public static Error NotFound => Error.NotFound(
            code: "State.NotFound",
            message: "The specified state was not found.");

        // Validate: Name rules
        public static Error NameRequired => Error.Validation(
            code: "State.Name.Required",
            message: "State name cannot be empty.");

        public static Error NameTooLong => Error.Validation(
            code: "State.Name.TooLong",
            message:
            $"State name cannot exceed {StateConstant.Constraints.MaxNameLength} characters.");

        // Validate: Abbreviation rules
        public static Error AbbreviationRequired => Error.Validation(
            code: "State.Abbreviation.Required",
            message: "State abbreviation cannot be empty.");

        public static Error AbbreviationTooLong => Error.Validation(
            code: "State.Abbreviation.TooLong",
            message:
            $"State abbreviation cannot exceed {StateConstant.Constraints.MaxAbbreviationLength} characters.");

        public static Error AbbreviationDuplicate => Error.Conflict(
            code: "State.Abbreviation.Duplicate",
            message: "A state with the same abbreviation already exists in this country.");

        // Validate: Country relationship rules
        public static Error CountryRequired => Error.Validation(
            code: "State.Country.Required",
            message: "Country is required for a state.");

        public static Error CountryNotFound => Error.NotFound(
            code: "State.Country.NotFound",
            message: "The specified country was not found.");

        // Validate: IsActive rules
        public static Error Inactive => Error.Validation(
            code: "State.Inactive",
            message: "The selected state is not active.");

        public static Error IdRequired => Error.Validation(
            code: "State.Id.Required",
            message: "State ID is required.");
    }
}
