namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Represents every comparison operator supported by the filter DSL.
/// </summary>
/// <remarks>
/// Members are mapped to and from DSL tokens and JSON aliases via
/// <see cref="FilterOperatorMap"/>. Do not add new members here without a
/// corresponding entry in that map.
/// </remarks>
public enum FilterOperator
{
    #region Equality

    /// <summary>Case-insensitive equality. DSL: <c>=</c> / JSON: <c>eq</c>.</summary>
    Equal,

    /// <summary>Case-sensitive equality. DSL: <c>==</c> / JSON: <c>eq~</c>.</summary>
    EqualCaseSensitive,

    /// <summary>Case-insensitive inequality. DSL: <c>!=</c> / JSON: <c>neq</c>.</summary>
    NotEqual,

    #endregion Equality

    #region Range

    /// <summary>Greater-than comparison. DSL: <c>&gt;</c> / JSON: <c>gt</c>.</summary>
    GreaterThan,

    /// <summary>Greater-than-or-equal comparison. DSL: <c>&gt;=</c> / JSON: <c>gte</c>.</summary>
    GreaterThanOrEqual,

    /// <summary>Less-than comparison. DSL: <c>&lt;</c> / JSON: <c>lt</c>.</summary>
    LessThan,

    /// <summary>Less-than-or-equal comparison. DSL: <c>&lt;=</c> / JSON: <c>lte</c>.</summary>
    LessThanOrEqual,

    #endregion Range

    #region String — Contains

    /// <summary>Case-insensitive contains. DSL: <c>*</c> / JSON: <c>contains</c>.</summary>
    Contains,

    /// <summary>Case-sensitive contains. DSL: <c>*~</c> / JSON: <c>contains~</c>.</summary>
    ContainsCaseSensitive,

    /// <summary>Case-insensitive does-not-contain. DSL: <c>!*</c> / JSON: <c>ncontains</c>.</summary>
    NotContains,

    #endregion String — Contains

    #region String — StartsWith

    /// <summary>Case-insensitive starts-with. DSL: <c>^</c> / JSON: <c>starts</c>.</summary>
    StartsWith,

    /// <summary>Case-sensitive starts-with. DSL: <c>^~</c> / JSON: <c>starts~</c>.</summary>
    StartsWithCaseSensitive,

    /// <summary>Case-insensitive does-not-start-with. DSL: <c>!^</c> / JSON: <c>nstarts</c>.</summary>
    NotStartsWith,

    #endregion String — StartsWith

    #region String — EndsWith

    /// <summary>Case-insensitive ends-with. DSL: <c>$</c> / JSON: <c>ends</c>.</summary>
    EndsWith,

    /// <summary>Case-sensitive ends-with. DSL: <c>$~</c> / JSON: <c>ends~</c>.</summary>
    EndsWithCaseSensitive,

    /// <summary>Case-insensitive does-not-end-with. DSL: <c>!$</c> / JSON: <c>nends</c>.</summary>
    NotEndsWith,

    #endregion String — EndsWith
}