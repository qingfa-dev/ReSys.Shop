namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// Typed error definitions for <see cref="SortModel"/> parse and validation failures.
/// </summary>
public static class SortModelResult
{
    #region Success

    /// <summary>
    /// Success messages for <see cref="SortModel"/> parse and construction operations.
    /// </summary>
    public static class Success
    {
        public static string Parsed => "Sort parsed successfully.";

        public static string Empty => "No sort input provided; empty model returned.";
    }

    #endregion Success

    #region Error

    /// <summary>
    /// Errors produced during parsing or field-whitelist validation of a <see cref="SortModel"/>.
    /// </summary>
    public static class Failure
    {
        /// <summary>
        /// The DSL sort string could not be parsed because its syntax is invalid.
        /// </summary>
        public static Error InvalidSyntax(string raw) =>
            Error.Validation(
                "Sorting.Parsing.InvalidSyntax",
                $"The sort string '{raw}' could not be parsed. Expected format: 'Field asc, Other desc'."
            );

        /// <summary>
        /// The JSON sort payload is missing a required property or has the wrong structure.
        /// </summary>
        public static Error InvalidJson(string detail) =>
            Error.Validation(
                "Sorting.Parsing.InvalidJson",
                $"The sort JSON is malformed: {detail}"
            );

        /// <summary>
        /// A clause references a field that is not in the entity's allowed sort fields.
        /// </summary>
        public static Error DisallowedField(string field) =>
            Error.Validation(
                "Sorting.Field.Disallowed",
                $"Field '{field}' is not allowed as a sort target."
            );

        /// <summary>
        /// One or more clauses reference disallowed fields; wraps all violations together.
        /// </summary>
        public static Error DisallowedFields(IEnumerable<string> fields) =>
            Error.Validation(
                "Sorting.Field.Disallowed",
                $"The following fields are not allowed as sort targets: {string.Join(", ", fields)}."
            );

        /// <summary>
        /// A direction value in JSON or query-string input is not a recognized <see cref="SortDirection"/>.
        /// </summary>
        public static Error UnknownDirection(string value) =>
            Error.Validation(
                "Sorting.Direction.Unknown",
                $"'{value}' is not a recognized sort direction. Use 'asc' or 'desc'."
            );

        /// <summary>
        /// A nulls-placement value in JSON input is not a recognized <see cref="SortNulls"/>.
        /// </summary>
        public static Error UnknownNulls(string value) =>
            Error.Validation(
                "Sorting.Nulls.Unknown",
                $"'{value}' is not a recognized nulls placement. Use 'first' or 'last'."
            );

        /// <summary>
        /// A clause in structured input is missing a required field name.
        /// </summary>
        public static Error MissingField =>
            Error.Validation(
                "Sorting.Field.Missing",
                "A sort clause is missing a required 'field' property."
            );
    }

    #endregion Error
}