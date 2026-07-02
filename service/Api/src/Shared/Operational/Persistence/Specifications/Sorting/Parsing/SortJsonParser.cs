using System.Text.Json;

namespace Shared.Operational.Persistence.Specifications.Sorting.Parsing;

/// <summary>
/// Parses a single JSON element into a <see cref="SortClause"/>.
/// </summary>
internal static class SortJsonParser
{
    /// <summary>
    /// Parses a JSON element representing a sort clause object:
    /// <c>{ "field": "Name", "direction": "desc", "nulls": "last" }</c>.
    /// Only <c>field</c> is required; <c>direction</c> defaults to ascending, <c>nulls</c> is optional.
    /// </summary>
    public static Result<SortClause> Parse(JsonElement element)
    {
        // Guard: "field" is required.
        if (!element.TryGetProperty("field", out JsonElement fieldEl))
            return SortModelResult.Failure.MissingField;

        string? field = fieldEl.GetString()?.Trim();
        if (string.IsNullOrEmpty(field))
            return SortModelResult.Failure.MissingField;

        // Parse: "direction" — defaults to ascending.
        SortDirection direction = SortDirection.Ascending;
        if (element.TryGetProperty("direction", out JsonElement dirEl))
        {
            Result<SortDirection> dirResult = SortParserHelpers.ParseDirection(dirEl.GetString());
            if (dirResult.IsFailure) return dirResult.Errors;
            direction = dirResult.Value;
        }

        // Parse: "nulls" — optional.
        SortNulls? nulls = null;
        if (element.TryGetProperty("nulls", out JsonElement nullsEl))
        {
            Result<SortNulls> nullsResult = SortParserHelpers.ParseNulls(nullsEl.GetString());
            if (nullsResult.IsFailure) return nullsResult.Errors;
            nulls = nullsResult.Value;
        }

        return new SortClause(field, direction, nulls);
    }
}
