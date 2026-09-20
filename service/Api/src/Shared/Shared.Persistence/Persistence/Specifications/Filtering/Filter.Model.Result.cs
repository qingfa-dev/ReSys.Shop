namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Typed result messages and error definitions for <see cref="FilterModel"/> operations.
/// </summary>
public static class FilterModelResult
{
    #region Success

    /// <summary>
    /// Informational messages returned on successful parse operations.
    /// </summary>
    public static class Success
    {
        /// <summary>The filter string was parsed successfully and produced a non-empty model.</summary>
        public static string Parsed => "Filter parsed successfully.";

        /// <summary>The input was empty or whitespace; an empty model was returned.</summary>
        public static string Empty => "No filter input provided; empty model returned.";
    }

    #endregion Success

    #region Error

    /// <summary>
    /// Typed <see cref="Error"/> instances for every parse and validation Error path.
    /// Error codes follow the <c>Filter.Property.ErrorType</c> hierarchy.
    /// </summary>
    public static class Failure
    {
        // ── DSL string ────────────────────────────────────────────────────────────

        /// <summary>
        /// The DSL filter string could not be tokenized due to invalid syntax.
        /// </summary>
        public static Error InvalidSyntax(string raw) =>
            Error.Validation(
                "Filter.String.InvalidSyntax",
                $"The filter string '{raw}' could not be parsed. Check operators, quotes, and parentheses."
            );

        // ── JSON ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// The JSON filter payload is malformed or has an unexpected structure.
        /// </summary>
        public static Error InvalidJson(string detail) =>
            Error.Validation(
                "Filter.Json.InvalidStructure",
                $"The filter JSON is malformed: {detail}"
            );

        // ── Operator ──────────────────────────────────────────────────────────────

        /// <summary>
        /// An operator token in structured input (JSON or query string) is not recognized.
        /// </summary>
        public static Error UnknownOperator(string token) =>
            Error.Validation(
                "Filter.Operator.Unknown",
                $"'{token}' is not a recognized filter operator."
            );

        // ── Field ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// A condition in structured input is missing a required field name.
        /// </summary>
        public static Error MissingField =>
            Error.Validation(
                "Filter.Field.Missing",
                "A filter condition is missing the required 'field' property."
            );

        /// <summary>
        /// A condition references a single field not present in the allowed-fields whitelist.
        /// </summary>
        public static Error DisallowedField(string field) =>
            Error.Validation(
                "Filter.Field.Disallowed",
                $"Field '{field}' is not permitted as a filter target."
            );

        /// <summary>
        /// One or more conditions reference fields not present in the allowed-fields whitelist.
        /// Aggregates all violations into a single Error.
        /// </summary>
        public static Error DisallowedFields(IEnumerable<string> fields) =>
            Error.Validation(
                "Filter.Field.Disallowed",
                $"The following fields are not permitted as filter targets: {string.Join(", ", fields)}."
            );

        // ── Operator presence ─────────────────────────────────────────────────────

        /// <summary>
        /// A condition in structured input is missing a required operator.
        /// </summary>
        public static Error MissingOperator =>
            Error.Validation(
                "Filter.Operator.Missing",
                "A filter condition is missing the required 'op' property."
            );

        // ── Query string ──────────────────────────────────────────────────────────

        /// <summary>
        /// A query-string filter entry does not conform to the <c>field:op:value</c> triplet format.
        /// </summary>
        public static Error InvalidTriplet(string entry) =>
            Error.Validation(
                "Filter.QueryString.InvalidTriplet",
                $"Query-string filter '{entry}' must follow the 'field:op:value' format."
            );
    }

    #endregion Error
}