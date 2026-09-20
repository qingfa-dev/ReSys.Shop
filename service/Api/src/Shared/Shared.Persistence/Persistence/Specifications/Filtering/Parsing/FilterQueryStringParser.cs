namespace Shared.Operational.Persistence.Specifications.Filtering.Parsing;

/// <summary>
/// Parses a collection of colon-separated <c>field:op:value</c> triplets into a
/// flat <see cref="FilterGroup"/>.
/// </summary>
internal static class FilterQueryStringParser
{
    /// <summary>
    /// Entry point. Returns an empty group for null/empty input, a flat AND group on success,
    /// or a typed failure for the first malformed triplet.
    /// </summary>
    internal static Result<FilterGroup> Parse(IEnumerable<string?>? values)
    {
        string[] entries = (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .ToArray();

        // Check: No entries → return empty sentinel.
        if (entries.Length == 0)
            return FilterGroup.Empty;

        var conditions = new List<FilterCondition>(entries.Length);

        foreach (string entry in entries)
        {
            Result<FilterCondition> result = ParseTriplet(entry);
            if (result.IsFailure) return result.Errors;
            conditions.Add(result.Value);
        }

        return FilterGroup.FlatAnd(conditions.AsReadOnly());
    }

    // ── Triplet ───────────────────────────────────────────────────────────────

    private static Result<FilterCondition> ParseTriplet(string entry)
    {
        // Split: Maximum of 3 parts so values containing colons (e.g. ISO timestamps) survive intact.
        string[] parts = entry.Split(
            FilterModelConstant.Defaults.QueryStringSeparator,
            FilterModelConstant.Defaults.QueryStringSplitCount);

        if (parts.Length < 2)
            return FilterModelResult.Failure.InvalidTriplet(entry);

        string field = parts[0].Trim();
        string opToken = parts[1].Trim();
        string value = parts.Length == 3 ? parts[2].Trim() : string.Empty;

        // Guard: Field must be non-empty.
        if (string.IsNullOrEmpty(field))
            return FilterModelResult.Failure.MissingField;

        // Guard: Operator must be recognized.
        if (!FilterOperatorMap.TryParse(opToken, out FilterOperator op))
            return FilterModelResult.Failure.UnknownOperator(opToken);

        return new FilterCondition { Field = field, Operator = op, Value = value };
    }
}
