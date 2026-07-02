using Shared.Operational.Persistence.Specifications.Filtering.Parsing;
using Shared.Security.Cors.Options;

namespace Shared.Operational.Persistence.Specifications.Filtering.Extensions;

/// <summary>
/// Public factory methods that parse raw input from any supported surface into a
/// validated <see cref="FilterModel"/>.
/// </summary>
/// <remarks>
/// Three input surfaces are supported — all produce the same <see cref="FilterModel"/> shape:
/// <list type="bullet">
///   <item><description>
///     <see cref="FromString"/> — DSL string
///     (e.g. <c>Name = *John*, Age &gt; 18, (Status = Active | Status = Pending)</c>)
///   </description></item>
///   <item><description>
///     <see cref="FromJson"/> — JSON array of condition/group objects
///   </description></item>
///   <item><description>
///     <see cref="FromQueryString"/> — colon-separated <c>field:op:value</c> triplets
///   </description></item>
/// </list>
/// All overloads accept an optional <c>allowedFields</c> whitelist. Violations are
/// surfaced in <see cref="FilterModel.Violations"/> rather than thrown.
/// </remarks>
public static class FilterModelExtensions
{
    #region FromString — DSL

    /// <summary>
    /// Parses a DSL filter string into a <see cref="FilterModel"/>.
    /// </summary>
    /// <param name="filterString">
    /// The DSL string (e.g. <c>Name = *John*, Age &gt; 18, (Status = Active | Status = Pending)</c>).
    /// AND is expressed via <c>,</c>; OR via <c>|</c>; grouping via <c>( )</c>.
    /// </param>
    /// <param name="allowedFields">
    /// Optional whitelist of permitted field names. Violations populate
    /// <see cref="FilterModel.Violations"/> and set <see cref="FilterModel.IsValid"/>
    /// to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see cref="FilterModel.Empty"/> for blank input; a parsed model on success;
    /// a <see cref="CorsResult.Errors.InvalidSyntax"/> failure otherwise.
    /// </returns>
    public static Result<FilterModel> FromString(
        string? filterString,
        IReadOnlySet<string>? allowedFields = null)
    {
        // Check: Blank input is valid — return the shared empty sentinel.
        if (string.IsNullOrWhiteSpace(filterString))
            return FilterModel.Empty;

        try
        {
            // Parse: Tokenize the DSL string into a group tree.
            FilterGroup root = FilterDslParser.Parse(filterString);

            // Build: Construct model with optional whitelist enforcement.
            return new FilterModel(root, allowedFields, rawInput: filterString);
        }
        catch
        {
            // Recover: Surface a typed failure rather than propagating the exception.
            return FilterModelResult.Failure.InvalidSyntax(filterString);
        }
    }

    /// <summary>
    /// Convenience overload accepting the whitelist as a plain string array —
    /// compatible with <c>[Entity]Constant.Query.AllowedFilterFields</c> directly.
    /// </summary>
    public static Result<FilterModel> FromString(
        string? filterString,
        string[]? allowedFields)
        => FromString(filterString, ToSet(allowedFields));

    #endregion FromString — DSL

    #region FromJson — JSON array

    /// <summary>
    /// Parses a JSON array of condition / group objects into a <see cref="FilterModel"/>.
    /// </summary>
    /// <param name="json">
    /// A JSON array. Each element is either a flat condition object:
    /// <code>{ "field": "Name", "op": "contains", "value": "John" }</code>
    /// or a group object with a nested conditions array:
    /// <code>{ "logic": "or", "conditions": [ ... ] }</code>
    /// Top-level elements are combined with AND. Nesting is supported to arbitrary depth.
    /// </param>
    /// <param name="allowedFields">Optional whitelist of permitted field names.</param>
    /// <returns>
    /// <see cref="FilterModel.Empty"/> for blank input; a parsed model on success;
    /// a <see cref="CorsResult.Errors.InvalidJson"/> or structural failure otherwise.
    /// </returns>
    public static Result<FilterModel> FromJson(
        string? json,
        IReadOnlySet<string>? allowedFields = null)
    {
        // Check: Blank input returns the shared empty sentinel.
        if (string.IsNullOrWhiteSpace(json))
            return FilterModel.Empty;

        // Parse: Delegate all JSON traversal to the internal parser.
        Result<FilterGroup> groupResult = FilterJsonParser.Parse(json);
        if (groupResult.IsFailure) return groupResult.Errors;

        return new FilterModel(groupResult.Value, allowedFields, rawInput: json);
    }

    /// <summary>
    /// Convenience overload accepting the whitelist as a plain string array.
    /// </summary>
    public static Result<FilterModel> FromJson(
        string? json,
        string[]? allowedFields)
        => FromJson(json, ToSet(allowedFields));

    #endregion FromJson — JSON array

    #region FromQueryString — colon-separated triplets

    /// <summary>
    /// Parses a collection of colon-separated <c>field:op:value</c> triplets from a
    /// query string into a <see cref="FilterModel"/>.
    /// </summary>
    /// <param name="values">
    /// Each element must be a <c>field:op:value</c> triplet
    /// (e.g. <c>"Name:contains:John"</c>, <c>"CreatedAt:gte:2024-01-01T00:00:00Z"</c>).
    /// Multiple values are combined with AND at the root level.
    /// Values may themselves contain colons — splitting stops after the third segment.
    /// </param>
    /// <param name="allowedFields">Optional whitelist of permitted field names.</param>
    /// <returns>
    /// <see cref="FilterModel.Empty"/> for empty input; a parsed model on success;
    /// a typed failure for the first malformed triplet or unknown operator encountered.
    /// </returns>
    /// <remarks>
    /// Designed for direct binding from minimal-API endpoints:
    /// <code>
    /// string[] values = context.Request.Query["filter"].ToArray();
    /// Result&lt;FilterModel&gt; result = FilterModelExtensions.FromQueryString(values, allowedFields);
    /// </code>
    /// </remarks>
    public static Result<FilterModel> FromQueryString(
        IEnumerable<string?>? values,
        IReadOnlySet<string>? allowedFields = null)
    {
        // Parse: Delegate triplet parsing to the internal parser.
        Result<FilterGroup> groupResult = FilterQueryStringParser.Parse(values);
        if (groupResult.IsFailure) return groupResult.Errors;

        // Check: Empty group means no values were supplied.
        if (groupResult.Value.IsEmpty) return FilterModel.Empty;

        return new FilterModel(
            groupResult.Value,
            allowedFields,
            rawInput: null);
    }

    /// <summary>
    /// Convenience overload accepting the whitelist as a plain string array.
    /// </summary>
    public static Result<FilterModel> FromQueryString(
        IEnumerable<string?>? values,
        string[]? allowedFields)
        => FromQueryString(values, ToSet(allowedFields));

    #endregion FromQueryString — colon-separated triplets

    #region Private Helpers

    /// <summary>
    /// Converts a nullable string array into a case-insensitive read-only set,
    /// or returns <see langword="null"/> when the array is null or empty.
    /// </summary>
    private static HashSet<string>? ToSet(string[]? allowedFields)
        => allowedFields is { Length: > 0 }
            ? new HashSet<string>(allowedFields, StringComparer.OrdinalIgnoreCase)
            : null;

    #endregion Private Helpers
}