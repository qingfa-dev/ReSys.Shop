using System.Collections.ObjectModel;

namespace Shared.Operational.Persistence.Specifications.Sorting;

public partial class SortModel
{
    /// <summary>
    /// Returns <see langword="true"/> if any clause targets the specified <paramref name="field"/>.
    /// Comparison is case-insensitive.
    /// </summary>
    public bool HasField(string field)
        => Clauses.Any(c => c.Field.Equals(field, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the first clause that targets the specified <paramref name="field"/>,
    /// or <see langword="null"/> if no such clause exists. Comparison is case-insensitive.
    /// </summary>
    public SortClause? ClauseFor(string field)
        => Clauses.FirstOrDefault(c => c.Field.Equals(field, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the effective clauses to sort by: <see cref="Clauses"/> when non-empty,
    /// otherwise falls back to the supplied <paramref name="defaultClauses"/>.
    /// </summary>
    /// <param name="defaultClauses">
    /// The entity-level default sort clauses, typically from
    /// <c>[Entity]Constant.Query.DefaultSort</c>.
    /// </param>
    public IReadOnlyList<SortClause> ResolveClauses(IReadOnlyList<SortClause> defaultClauses)
        => Clauses.Count > 0 ? Clauses : defaultClauses;

    /// <summary>
    /// Computes the set of field names present in <paramref name="clauses"/> that are
    /// not in <paramref name="allowedFields"/>. Returns an empty collection when
    /// <paramref name="allowedFields"/> is <see langword="null"/>.
    /// </summary>
    private static ReadOnlyCollection<string> ComputeViolations(
        IReadOnlySet<string>? allowedFields,
        IReadOnlyList<SortClause> clauses)
    {
        // Validate: Check every referenced field against the whitelist.
        return allowedFields is null
            ? new List<string>().AsReadOnly()
            : clauses
                .Select(c => c.Field)
                .Where(f => !allowedFields.Contains(f, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
    }
}
