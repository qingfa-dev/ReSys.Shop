namespace Shared.Operational.Persistence.Specifications.Sorting.Parsing;

/// <summary>
/// Parses a single DSL sort clause segment (comma-separated token from <see cref="SortModelExtensions.FromString"/>).
/// </summary>
internal static class SortDslParser
{
    /// <summary>
    /// Parses a single DSL clause segment into a <see cref="SortClause"/>.
    /// Handles: <c>Field</c>, <c>Field asc</c>, <c>Field desc</c>, <c>+Field</c>, <c>-Field</c>.
    /// </summary>
    public static Result<SortClause> Parse(string segment)
    {
        // Handle: Direction prefix shorthand (+Field / -Field).
        if (segment.StartsWith('+') || segment.StartsWith('-'))
        {
            SortDirection prefixDir = segment[0] == '-'
                ? SortDirection.Descending
                : SortDirection.Ascending;

            string prefixField = segment[1..].Trim();
            if (string.IsNullOrEmpty(prefixField))
                return SortModelResult.Failure.MissingField;

            return new SortClause { Field = prefixField, Direction = prefixDir };
        }

        // Handle: "Field" or "Field direction" separated by whitespace.
        string[] tokens = segment.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0 || string.IsNullOrEmpty(tokens[0]))
            return SortModelResult.Failure.MissingField;

        string field = tokens[0];

        if (tokens.Length == 1)
            return new SortClause { Field = field, Direction = SortDirection.Ascending };

        Result<SortDirection> dirResult = SortParserHelpers.ParseDirection(tokens[1].Trim());
        if (dirResult.IsFailure) return dirResult.Errors;

        return new SortClause { Field = field, Direction = dirResult.Value };
    }
}
