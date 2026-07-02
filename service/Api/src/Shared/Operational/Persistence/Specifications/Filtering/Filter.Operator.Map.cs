namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Bidirectional mapping between <see cref="FilterOperator"/> members, their DSL tokens,
/// and their JSON / query-string aliases.
/// </summary>
/// <remarks>
/// This is the single authoritative source for all operator parsing and serialization.
/// Every new <see cref="FilterOperator"/> member requires a corresponding entry in both
/// <see cref="_dslTokens"/> and <see cref="_parseTable"/> before it can be used.
/// </remarks>
internal static class FilterOperatorMap
{
    #region Tables

    /// <summary>Maps each <see cref="FilterOperator"/> to its canonical DSL token.</summary>
    private static readonly Dictionary<FilterOperator, string> _dslTokens = new()
    {
        [FilterOperator.Equal]                   = FilterOperatorConstant.Tokens.Equal,
        [FilterOperator.EqualCaseSensitive]      = FilterOperatorConstant.Tokens.EqualCaseSensitive,
        [FilterOperator.NotEqual]                = FilterOperatorConstant.Tokens.NotEqual,
        [FilterOperator.GreaterThan]             = FilterOperatorConstant.Tokens.GreaterThan,
        [FilterOperator.GreaterThanOrEqual]      = FilterOperatorConstant.Tokens.GreaterThanOrEqual,
        [FilterOperator.LessThan]                = FilterOperatorConstant.Tokens.LessThan,
        [FilterOperator.LessThanOrEqual]         = FilterOperatorConstant.Tokens.LessThanOrEqual,
        [FilterOperator.Contains]                = FilterOperatorConstant.Tokens.Contains,
        [FilterOperator.ContainsCaseSensitive]   = FilterOperatorConstant.Tokens.ContainsCaseSensitive,
        [FilterOperator.NotContains]             = FilterOperatorConstant.Tokens.NotContains,
        [FilterOperator.StartsWith]              = FilterOperatorConstant.Tokens.StartsWith,
        [FilterOperator.StartsWithCaseSensitive] = FilterOperatorConstant.Tokens.StartsWithCaseSensitive,
        [FilterOperator.NotStartsWith]           = FilterOperatorConstant.Tokens.NotStartsWith,
        [FilterOperator.EndsWith]                = FilterOperatorConstant.Tokens.EndsWith,
        [FilterOperator.EndsWithCaseSensitive]   = FilterOperatorConstant.Tokens.EndsWithCaseSensitive,
        [FilterOperator.NotEndsWith]             = FilterOperatorConstant.Tokens.NotEndsWith,
    };

    /// <summary>
    /// All recognized tokens (DSL symbols + JSON aliases) keyed case-insensitively to
    /// their <see cref="FilterOperator"/> equivalent.
    /// </summary>
    private static readonly Dictionary<string, FilterOperator> _parseTable =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Equality
            [FilterOperatorConstant.Tokens.Equal]                   = FilterOperator.Equal,
            [FilterOperatorConstant.Tokens.EqualAlias]              = FilterOperator.Equal,
            [FilterOperatorConstant.Tokens.EqualCaseSensitive]      = FilterOperator.EqualCaseSensitive,
            [FilterOperatorConstant.Tokens.EqualCaseSensitiveAlias] = FilterOperator.EqualCaseSensitive,
            [FilterOperatorConstant.Tokens.NotEqual]                = FilterOperator.NotEqual,
            [FilterOperatorConstant.Tokens.NotEqualAlias]           = FilterOperator.NotEqual,

            // Range
            [FilterOperatorConstant.Tokens.GreaterThan]             = FilterOperator.GreaterThan,
            [FilterOperatorConstant.Tokens.GreaterThanAlias]        = FilterOperator.GreaterThan,
            [FilterOperatorConstant.Tokens.GreaterThanOrEqual]      = FilterOperator.GreaterThanOrEqual,
            [FilterOperatorConstant.Tokens.GreaterThanOrEqualAlias] = FilterOperator.GreaterThanOrEqual,
            [FilterOperatorConstant.Tokens.LessThan]                = FilterOperator.LessThan,
            [FilterOperatorConstant.Tokens.LessThanAlias]           = FilterOperator.LessThan,
            [FilterOperatorConstant.Tokens.LessThanOrEqual]         = FilterOperator.LessThanOrEqual,
            [FilterOperatorConstant.Tokens.LessThanOrEqualAlias]    = FilterOperator.LessThanOrEqual,

            // Contains
            [FilterOperatorConstant.Tokens.Contains]                = FilterOperator.Contains,
            [FilterOperatorConstant.Tokens.ContainsAlias]           = FilterOperator.Contains,
            [FilterOperatorConstant.Tokens.ContainsCaseSensitive]      = FilterOperator.ContainsCaseSensitive,
            [FilterOperatorConstant.Tokens.ContainsCaseSensitiveAlias] = FilterOperator.ContainsCaseSensitive,
            [FilterOperatorConstant.Tokens.NotContains]                = FilterOperator.NotContains,
            [FilterOperatorConstant.Tokens.NotContainsAlias]           = FilterOperator.NotContains,

            // StartsWith
            [FilterOperatorConstant.Tokens.StartsWith]                 = FilterOperator.StartsWith,
            [FilterOperatorConstant.Tokens.StartsWithAlias]            = FilterOperator.StartsWith,
            [FilterOperatorConstant.Tokens.StartsWithCaseSensitive]      = FilterOperator.StartsWithCaseSensitive,
            [FilterOperatorConstant.Tokens.StartsWithCaseSensitiveAlias] = FilterOperator.StartsWithCaseSensitive,
            [FilterOperatorConstant.Tokens.NotStartsWith]                = FilterOperator.NotStartsWith,
            [FilterOperatorConstant.Tokens.NotStartsWithAlias]           = FilterOperator.NotStartsWith,

            // EndsWith
            [FilterOperatorConstant.Tokens.EndsWith]                    = FilterOperator.EndsWith,
            [FilterOperatorConstant.Tokens.EndsWithAlias]               = FilterOperator.EndsWith,
            [FilterOperatorConstant.Tokens.EndsWithCaseSensitive]      = FilterOperator.EndsWithCaseSensitive,
            [FilterOperatorConstant.Tokens.EndsWithCaseSensitiveAlias] = FilterOperator.EndsWithCaseSensitive,
            [FilterOperatorConstant.Tokens.NotEndsWith]                = FilterOperator.NotEndsWith,
            [FilterOperatorConstant.Tokens.NotEndsWithAlias]           = FilterOperator.NotEndsWith,
        };

    #endregion Tables

    #region Lookup

    /// <summary>
    /// Returns the canonical DSL token for <paramref name="op"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="op"/> has no registered token — indicates a missing
    /// <see cref="_dslTokens"/> entry for a newly added enum member.
    /// </exception>
    internal static string ToDslToken(FilterOperator op)
        => _dslTokens.TryGetValue(op, out string? token)
            ? token
            : throw new ArgumentOutOfRangeException(nameof(op), op, "No DSL token registered for this FilterOperator.");

    /// <summary>
    /// Attempts to resolve a DSL token or JSON alias to its <see cref="FilterOperator"/>.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="token">The raw string to resolve.</param>
    /// <param name="op">When successful, the resolved operator.</param>
    /// <returns><see langword="true"/> if the token is recognized.</returns>
    internal static bool TryParse(string? token, out FilterOperator op)
    {
        op = default;
        return token is not null && _parseTable.TryGetValue(token, out op);
    }

    #endregion Lookup

    #region Classification

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="op"/> performs case-sensitive matching.
    /// </summary>
    internal static bool IsCaseSensitive(FilterOperator op) => op is
        FilterOperator.EqualCaseSensitive      or
        FilterOperator.ContainsCaseSensitive   or
        FilterOperator.StartsWithCaseSensitive or
        FilterOperator.EndsWithCaseSensitive;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="op"/> is a negation variant.
    /// </summary>
    internal static bool IsNegation(FilterOperator op) => op is
        FilterOperator.NotEqual      or
        FilterOperator.NotContains   or
        FilterOperator.NotStartsWith or
        FilterOperator.NotEndsWith;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="op"/> is a string-only operator
    /// (contains / starts-with / ends-with and their variants).
    /// </summary>
    internal static bool IsStringOnly(FilterOperator op) => op is
        FilterOperator.Contains              or
        FilterOperator.ContainsCaseSensitive or
        FilterOperator.NotContains           or
        FilterOperator.StartsWith            or
        FilterOperator.StartsWithCaseSensitive or
        FilterOperator.NotStartsWith         or
        FilterOperator.EndsWith              or
        FilterOperator.EndsWithCaseSensitive or
        FilterOperator.NotEndsWith;

    #endregion Classification
}