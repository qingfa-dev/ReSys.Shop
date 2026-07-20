namespace Shared.Operational.Persistence.Specifications.Sorting;

/// <summary>
/// Represents a single sort clause: a field name, a direction, and an optional null-placement rule.
/// </summary>
/// <remarks>
/// <c>SortClause</c> is the leaf unit of a <see cref="SortModel"/>. The ordering of clauses
/// in <see cref="SortModel.Clauses"/> determines sort priority — index 0 is the primary sort,
/// index 1 is the tie-breaker, and so on.
/// </remarks>
public sealed partial record SortClause
{
    /// <summary>
    /// The property name to sort by. Supports dot-notation for navigation properties
    /// (e.g. <c>"Order.CreatedAt"</c>). Accepts snake_case, camelCase, or PascalCase —
    /// the expression builder resolves casing at runtime.
    /// </summary>
    public string Field { get; init; } = default!;

    /// <summary>
    /// The sort direction. Defaults to <see cref="SortDirection.Ascending"/>.
    /// </summary>
    public SortDirection Direction { get; init; } = SortDirection.Ascending;

    /// <summary>
    /// Optional null-placement rule. <see langword="null"/> means the engine default applies.
    /// </summary>
    public SortNulls? Nulls { get; init; } = null;

    #region Constants

    public static class Constant
    {
        public const SortDirection DefaultDirection = SortDirection.Ascending;

        public static SortNulls? DefaultNulls => null;
    }

    #endregion Constants

    #region Display

    /// <summary>
    /// Gets the canonical DSL token for this clause's direction: <c>"asc"</c> or <c>"desc"</c>.
    /// </summary>
    public string DirectionToken => Direction == SortDirection.Descending ? "desc" : "asc";

    /// <summary>
    /// Returns a human-readable DSL representation of this clause,
    /// useful for logging and diagnostics (e.g. <c>"CreatedAt desc"</c>, <c>"Name asc nulls last"</c>).
    /// </summary>
    public override string ToString()
    {
        string clauseBase = $"{Field} {DirectionToken}";

        return Nulls switch
        {
            SortNulls.First => $"{clauseBase} nulls first",
            SortNulls.Last  => $"{clauseBase} nulls last",
            _               => clauseBase
        };
    }

    #endregion Display
}
