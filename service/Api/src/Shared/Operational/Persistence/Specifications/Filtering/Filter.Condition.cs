namespace Shared.Operational.Persistence.Specifications.Filtering;

/// <summary>
/// Represents a single atomic filter condition: a property name, a comparison operator,
/// and a string value to compare against.
/// </summary>
/// <param name="Field">
/// The property name to filter on. Supports dot-notation for navigation properties
/// (e.g. <c>"Order.Customer.Name"</c>). Accepts snake_case, camelCase, or PascalCase —
/// casing is resolved at expression-build time.
/// </param>
/// <param name="Operator">The comparison operator to apply.</param>
/// <param name="Value">
/// The raw string representation of the comparison value. <c>"null"</c> is treated as a
/// typed <see langword="null"/>; an empty string is treated as <see cref="string.Empty"/>
/// for string properties.
/// </param>
/// <remarks>
/// <c>FilterCondition</c> is the leaf node of the <see cref="FilterGroup"/> tree.
/// It carries no logical connective — that belongs to the parent
/// <see cref="FilterGroup.Logic"/>.
/// </remarks>
public sealed partial record FilterCondition(
    string Field,
    FilterOperator Operator,
    string Value
)
{
    #region Derived Properties

    /// <summary>
    /// Gets the canonical DSL token string for <see cref="Operator"/>.
    /// Delegates to <see cref="FilterOperatorMap.ToDslToken"/>.
    /// </summary>
    public string OperatorToken => FilterOperatorMap.ToDslToken(Operator);

    /// <summary>
    /// Returns <see langword="true"/> when the operator performs case-sensitive matching.
    /// </summary>
    public bool IsCaseSensitive => FilterOperatorMap.IsCaseSensitive(Operator);

    /// <summary>
    /// Returns <see langword="true"/> when the operator is a negation variant
    /// (not-equal, not-contains, not-starts-with, not-ends-with).
    /// </summary>
    public bool IsNegation => FilterOperatorMap.IsNegation(Operator);

    /// <summary>
    /// Returns <see langword="true"/> when the operator is valid only for string properties.
    /// </summary>
    public bool IsStringOnly => FilterOperatorMap.IsStringOnly(Operator);

    #endregion Derived Properties

    #region Display

    /// <summary>
    /// Returns a human-readable DSL representation of this condition.
    /// Intended for logging and diagnostics — not for round-trip parsing.
    /// </summary>
    /// <example><c>Name * john</c>, <c>Age &gt;= 18</c>, <c>Status = Active</c></example>
    public override string ToString() => $"{Field} {OperatorToken} {Value}";

    #endregion Display
}
