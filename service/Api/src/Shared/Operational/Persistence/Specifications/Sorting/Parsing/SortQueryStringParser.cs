namespace Shared.Operational.Persistence.Specifications.Sorting.Parsing;

/// <summary>
/// Parses a single query-string sort entry into a <see cref="SortClause"/>.
/// </summary>
/// <remarks>
/// Each entry is one of:
/// <list type="bullet">
///   <item><description><c>"Name"</c> — bare field name, defaults to ascending.</description></item>
///   <item><description><c>"Name:asc"</c> / <c>"Name:desc"</c> — colon-separated field and direction.</description></item>
///   <item><description><c>"+Name"</c> / <c>"-Name"</c> — direction-prefix shorthand.</description></item>
/// </list>
/// </remarks>
internal static class SortQueryStringParser
{
    /// <summary>
    /// Parses a single query-string sort entry into a <see cref="SortClause"/>.
    /// </summary>
    public static Result<SortClause> ParseEntry(string entry)
    {
        // Branch A: Direction prefix shorthand (+Field / -Field).
        if (entry.StartsWith('+') || entry.StartsWith('-'))
        {
            SortDirection dir = entry[0] == '-' ? SortDirection.Descending : SortDirection.Ascending;
            string field = entry[1..].Trim();

            if (string.IsNullOrEmpty(field))
                return SortModelResult.Failure.MissingField;

            return new SortClause { Field = field, Direction = dir };
        }

        // Branch B: Colon-separated field:direction pair.
        int colonIdx = entry.IndexOf(':');
        if (colonIdx >= 0)
        {
            string field = entry[..colonIdx].Trim();
            string dirToken = entry[(colonIdx + 1)..].Trim();

            if (string.IsNullOrEmpty(field))
                return SortModelResult.Failure.MissingField;

            Result<SortDirection> dirResult = SortParserHelpers.ParseDirection(dirToken);
            if (dirResult.IsFailure) return dirResult.Errors;

            return new SortClause { Field = field, Direction = dirResult.Value };
        }

        // Branch C: Bare field name — defaults to ascending.
        if (string.IsNullOrEmpty(entry))
            return SortModelResult.Failure.MissingField;

        return new SortClause { Field = entry, Direction = SortDirection.Ascending };
    }
}
