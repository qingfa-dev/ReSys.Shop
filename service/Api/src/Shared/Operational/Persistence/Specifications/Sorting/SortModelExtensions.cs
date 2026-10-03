using System.Text.Json;

using Shared.Operational.Persistence.Specifications.Sorting.Parsing;

namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// Factory methods that parse raw input into a validated <see cref="SortModel"/>.
/// </summary>
/// <remarks>
/// Three input surfaces are supported — all produce the same <see cref="SortModel"/> shape:
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="FromString"/> — a comma-separated DSL string where each clause is
///       <c>Field</c>, <c>Field asc</c>, <c>Field desc</c>, or direction-prefixed
///       <c>+Field</c> / <c>-Field</c> (e.g. <c>"Name asc, CreatedAt desc"</c>).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="FromJson"/> — a JSON array of clause objects with <c>field</c>,
///       optional <c>direction</c>, and optional <c>nulls</c>
///       (e.g. <c>[{"field":"Name"},{"field":"CreatedAt","direction":"desc","nulls":"last"}]</c>).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="FromQueryString"/> — one or more <c>sort</c> query parameters, each either
///       a bare field name (<c>?sort=Name</c>), a colon-separated <c>field:direction</c> pair
///       (<c>?sort=CreatedAt:desc</c>), or a direction-prefixed field (<c>?sort=-CreatedAt</c>).
///       Multiple values set sort priority in the order they appear.
///     </description>
///   </item>
/// </list>
/// <para>
/// All overloads accept an optional <c>allowedFields</c> whitelist enforced after parsing.
/// Violations surface in <see cref="SortModel.Violations"/> and cause
/// <see cref="SortModel.IsValid"/> to return <see langword="false"/> rather than throwing,
/// consistent with the fail-safe contract of the querying layer.
/// </para>
/// </remarks>
public static class SortModelExtensions
{
    #region DSL String

    /// <summary>
    /// Parses a comma-separated DSL sort string into a <see cref="SortModel"/>.
    /// </summary>
    /// <param name="sortString">
    /// A comma-separated list of sort clauses. Each clause may be one of:
    /// <list type="bullet">
    ///   <item><description><c>Name</c> — field only, defaults to ascending.</description></item>
    ///   <item><description><c>Name asc</c> / <c>Name desc</c> — explicit direction.</description></item>
    ///   <item><description><c>+Name</c> / <c>-Name</c> — direction prefix shorthand.</description></item>
    /// </list>
    /// Example: <c>"Name asc, CreatedAt desc, +Priority"</c>.
    /// </param>
    /// <param name="allowedFields">
    /// Optional whitelist of permitted field names. When supplied, clauses referencing
    /// any field not in this set cause <see cref="SortModel.IsValid"/> to return
    /// <see langword="false"/> and populate <see cref="SortModel.Violations"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{SortModel}"/> containing the parsed model on success,
    /// or a <see cref="SortModelResult.Failure.InvalidSyntax"/> failure if the string
    /// cannot be tokenized.
    /// </returns>
    public static Result<SortModel> FromString(
        string? sortString,
        IReadOnlySet<string>? allowedFields = null)
    {
        // Check: Empty input returns an empty model — never an error.
        if (string.IsNullOrWhiteSpace(sortString))
            return SortModel.Empty;

        try
        {
            List<SortClause> clauses = new();

            // Parse: Split on comma — each segment is one clause.
            foreach (string segment in sortString.Split(','))
            {
                string trimmed = segment.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                Result<SortClause> clauseResult = SortDslParser.Parse(trimmed);
                if (clauseResult.IsFailure) return clauseResult.Errors;

                clauses.Add(clauseResult.Value);
            }

            return clauses.Count == 0
                ? SortModel.Empty
                : new SortModel(clauses.AsReadOnly(), allowedFields, rawInput: sortString);
        }
        catch
        {
            return SortModelResult.Failure.InvalidSyntax(sortString);
        }
    }

    /// <summary>
    /// Convenience overload that accepts the whitelist as a plain string array,
    /// matching the shape of <c>[Entity]Constant.Query.AllowedSortFields</c>.
    /// </summary>
    public static Result<SortModel> FromString(
        string? sortString,
        string[]? allowedFields)
    {
        IReadOnlySet<string>? set = ToSet(allowedFields);
        return FromString(sortString, set);
    }

    #endregion DSL String

    #region JSON

    /// <summary>
    /// Parses a JSON array of sort clause objects into a <see cref="SortModel"/>.
    /// </summary>
    /// <param name="json">
    /// A JSON array where each element has the following shape:
    /// <code>
    /// [
    ///   { "field": "Name" },
    ///   { "field": "CreatedAt", "direction": "desc" },
    ///   { "field": "Priority",  "direction": "asc", "nulls": "last" }
    /// ]
    /// </code>
    /// Only <c>field</c> is required per element. <c>direction</c> defaults to <c>"asc"</c>;
    /// <c>nulls</c> is optional (<c>"first"</c> or <c>"last"</c>).
    /// Array order determines sort priority — index 0 is the primary sort key.
    /// </param>
    /// <param name="allowedFields">Optional whitelist of permitted field names.</param>
    /// <returns>
    /// A <see cref="Result{SortModel}"/> containing the parsed model on success,
    /// or a typed failure describing the first structural problem found.
    /// </returns>
    public static Result<SortModel> FromJson(
        string? json,
        IReadOnlySet<string>? allowedFields = null)
    {
        // Check: Empty input returns an empty model.
        if (string.IsNullOrWhiteSpace(json))
            return SortModel.Empty;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Guard: Root element must be an array.
            if (root.ValueKind != JsonValueKind.Array)
                return SortModelResult.Failure.InvalidJson("Root element must be a JSON array.");

            List<SortClause> clauses = new();

            foreach (JsonElement element in root.EnumerateArray())
            {
                Result<SortClause> clauseResult = SortJsonParser.Parse(element);
                if (clauseResult.IsFailure) return clauseResult.Errors;
                clauses.Add(clauseResult.Value);
            }

            return clauses.Count == 0
                ? SortModel.Empty
                : new SortModel(clauses.AsReadOnly(), allowedFields, rawInput: json);
        }
        catch (JsonException ex)
        {
            return SortModelResult.Failure.InvalidJson(ex.Message);
        }
        catch
        {
            return SortModelResult.Failure.InvalidJson("An unexpected error occurred while parsing the sort JSON.");
        }
    }

    /// <summary>
    /// Convenience overload that accepts the whitelist as a plain string array.
    /// </summary>
    public static Result<SortModel> FromJson(
        string? json,
        string[]? allowedFields)
    {
        IReadOnlySet<string>? set = ToSet(allowedFields);
        return FromJson(json, set);
    }

    #endregion JSON

    #region Query String

    /// <summary>
    /// Parses a collection of sort query-string values into a <see cref="SortModel"/>.
    /// </summary>
    /// <param name="values">
    /// Each element is one of:
    /// <list type="bullet">
    ///   <item><description><c>"Name"</c> — bare field name, defaults to ascending.</description></item>
    ///   <item><description><c>"Name:asc"</c> / <c>"Name:desc"</c> — colon-separated field and direction.</description></item>
    ///   <item><description><c>"+Name"</c> / <c>"-Name"</c> — direction-prefix shorthand.</description></item>
    /// </list>
    /// Array order determines sort priority — index 0 is the primary sort key.
    /// Designed to be called from minimal API endpoints:
    /// <code>
    /// var values = context.Request.Query["sort"].ToArray();
    /// var result = SortModelExtensions.FromQueryString(values, allowedFields);
    /// </code>
    /// </param>
    /// <param name="allowedFields">Optional whitelist of permitted field names.</param>
    /// <returns>
    /// A <see cref="Result{SortModel}"/> containing the parsed model on success,
    /// or a typed failure for the first malformed entry encountered.
    /// </returns>
    public static Result<SortModel> FromQueryString(
        IEnumerable<string?>? values,
        IReadOnlySet<string>? allowedFields = null)
    {
        string[] entries = (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .ToArray();

        // Check: No values → empty model.
        if (entries.Length == 0)
            return SortModel.Empty;

        List<SortClause> clauses = new(entries.Length);

        foreach (string entry in entries)
        {
            Result<SortClause> clauseResult = SortQueryStringParser.ParseEntry(entry);
            if (clauseResult.IsFailure) return clauseResult.Errors;
            clauses.Add(clauseResult.Value);
        }

        return clauses.Count == 0
            ? SortModel.Empty
            : new SortModel(clauses.AsReadOnly(), allowedFields, rawInput: string.Join(",", entries));
    }

    /// <summary>
    /// Convenience overload that accepts the whitelist as a plain string array.
    /// </summary>
    public static Result<SortModel> FromQueryString(
        IEnumerable<string?>? values,
        string[]? allowedFields)
    {
        IReadOnlySet<string>? set = ToSet(allowedFields);
        return FromQueryString(values, set);
    }

    #endregion Query String

    #region Utilities (private)

    private static HashSet<string>? ToSet(string[]? allowedFields) =>
        allowedFields is { Length: > 0 }
            ? new HashSet<string>(allowedFields, StringComparer.OrdinalIgnoreCase)
            : null;

    #endregion Utilities
}