namespace Shared.Operational.Persistence.Specifications.Sorting.Parsing;

/// <summary>
/// Shared parsing utilities used by all sort parser classes.
/// </summary>
internal static class SortParserHelpers
{
    /// <summary>
    /// Resolves a direction string to a <see cref="SortDirection"/>.
    /// Accepts <c>asc</c>, <c>ascending</c>, <c>desc</c>, <c>descending</c> (case-insensitive).
    /// </summary>
    internal static Result<SortDirection> ParseDirection(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "asc"  or "ascending"  or null or "" => SortDirection.Ascending,
            "desc" or "descending"               => SortDirection.Descending,
            _                                    => (Result<SortDirection>)SortModelResult.Failure.UnknownDirection(value!)
        };

    /// <summary>
    /// Resolves a nulls-placement string to a <see cref="SortNulls"/>.
    /// Accepts <c>first</c> and <c>last</c> (case-insensitive).
    /// </summary>
    internal static Result<SortNulls> ParseNulls(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "first" => SortNulls.First,
            "last"  => SortNulls.Last,
            _       => (Result<SortNulls>)SortModelResult.Failure.UnknownNulls(value ?? "(null)")
        };
}
