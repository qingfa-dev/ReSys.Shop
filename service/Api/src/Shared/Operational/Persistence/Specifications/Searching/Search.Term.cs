using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.Operational.Persistence.Specifications.Searching;

/// <summary>
/// Represents a single, normalized search term that has been validated and is ready
/// to be applied to a query.
/// </summary>
/// <param name="Value">
/// The trimmed, non-empty string to search for. Never <see langword="null"/> or whitespace —
/// construction is only permitted via <see cref="SearchModel"/> factory methods which enforce this.
/// </param>
/// <param name="CaseSensitive">
/// <see langword="true"/> if the match must be exact-case; <see langword="false"/> (default)
/// for case-insensitive matching using <c>ToLowerInvariant</c> normalization, consistent
/// with the filter layer's behavior for string operators.
/// </param>
/// <remarks>
/// A <c>SearchTerm</c> does not carry field information — it is field-agnostic.
/// The target fields and match mode are owned by the parent <see cref="SearchModel"/>.
/// <para>
/// <c>SearchTerm</c> intentionally has no operator: search is always a substring
/// (<em>contains</em>) match. For prefix/suffix or equality matching on a specific field,
/// use the filter layer (<see cref="FilterCondition"/>).
/// </para>
/// </remarks>
public sealed partial record SearchTerm(string Value, bool CaseSensitive = false)
{
    // AgentHint: EffectiveValue recomputes on every access; acceptable because it is a simple property.

    // Contract: Default values shared across all search term construction sites.
    public static class Constant
    {
        public const bool DefaultCaseSensitive = false;
        public const string CaseInsensitiveSuffix = "~";
        public static readonly SearchMode DefaultMode = SearchMode.Any;
    }

    /// <summary>
    /// Gets the effective search value used at query time:
    /// the lowercased <see cref="Value"/> for case-insensitive searches,
    /// or the original <see cref="Value"/> for case-sensitive ones.
    /// </summary>
    public string EffectiveValue => CaseSensitive
        ? Value
        : Value.ToLowerInvariant();

    /// <summary>
    /// Returns the term value as a diagnostic string, with a <c>~</c> suffix when case-insensitive.
    /// </summary>
    public override string ToString() => CaseSensitive ? Value : $"{Value}~";
}
