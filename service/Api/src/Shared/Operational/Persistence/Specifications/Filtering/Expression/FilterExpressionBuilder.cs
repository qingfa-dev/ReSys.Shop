using System.Linq.Expressions;
using System.Reflection;

using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.Operational.Persistence.Specifications.Filtering.Expression;

/// <summary>
/// Shared Expression-tree construction helpers used by the model-tree path
/// (<see cref="FilterGroupVisitor{T}"/>) when building predicates from
/// <see cref="FilterCondition"/> leaf nodes.
/// </summary>
/// <remarks>
/// All members are <see langword="internal static"/> — this class has no public surface.
/// Callers pass a root <see cref="ParameterExpression"/> representing the lambda parameter
/// <c>x</c> and receive back a nullable <see cref="Expression"/> that can be composed into
/// a larger predicate. <see langword="null"/> return values always mean "skip this condition"
/// (fail-safe), never throw.
/// </remarks>
internal static class FilterExpressionBuilder
{
    #region Entry Point

    /// <summary>
    /// Builds a single predicate <see cref="Expression"/> for the given field, operator,
    /// value string, and case-sensitivity flag, rooted at <paramref name="param"/>.
    /// </summary>
    /// <param name="param">The lambda parameter (e.g. <c>x</c> in <c>x => x.Name == "John"</c>).</param>
    /// <param name="fieldName">
    /// The property path to filter on. Dot-notation is supported for navigation properties
    /// (e.g. <c>"Order.Customer.Name"</c>).
    /// </param>
    /// <param name="op">
    /// The raw DSL operator token (e.g. <c>"="</c>, <c>"*"</c>, <c>"!^"</c>).
    /// </param>
    /// <param name="value">The raw string value to compare against.</param>
    /// <param name="caseSensitive">
    /// When <see langword="true"/>, string comparisons are performed without case-folding.
    /// </param>
    /// <returns>
    /// A predicate <see cref="Expression"/> ready to be composed, or <see langword="null"/>
    /// if the field does not exist on the type or the value cannot be parsed.
    /// </returns>
    internal static System.Linq.Expressions.Expression? Build<T>(
        ParameterExpression param,
        string fieldName,
        string op,
        string value,
        bool caseSensitive)
    {
        System.Linq.Expressions.Expression expr = param;
        Type type = typeof(T);
        System.Linq.Expressions.Expression? nullCheck = null;

        // Traverse: Walk dot-separated navigation path (e.g. "Order.Customer.Name").
        foreach (string member in fieldName.Split(FilterExpressionBuilderConstant.Navigation.Separator))
        {
            PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(type, member);
            if (property == null) return null;

            System.Linq.Expressions.Expression parentExpr = expr;
            expr = System.Linq.Expressions.Expression.Property(expr, property);
            type = property.PropertyType;

            // Guard: Accumulate null checks for intermediate reference/nullable navigation properties.
            if (parentExpr != param && (!parentExpr.Type.IsValueType || Nullable.GetUnderlyingType(parentExpr.Type) != null))
            {
                BinaryExpression notNull = System.Linq.Expressions.Expression.NotEqual(parentExpr, System.Linq.Expressions.Expression.Constant(null, parentExpr.Type));
                nullCheck = nullCheck == null ? notNull : System.Linq.Expressions.Expression.AndAlso(nullCheck, notNull);
            }
        }

        // Parse: Convert filter value string to the target property type.
        ConstantExpression? constExpr = ParseConstant(value, type);
        if (constExpr == null) return null;

        // Handle: Null comparisons (= null / != null) — skip the leaf null-safety guard.
        if (constExpr.Value == null)
        {
            System.Linq.Expressions.Expression? nullComparison = op switch
            {
                FilterOperatorConstant.Tokens.Equal => System.Linq.Expressions.Expression.Equal(expr, constExpr),
                FilterOperatorConstant.Tokens.NotEqual => System.Linq.Expressions.Expression.NotEqual(expr, constExpr),
                _ => null
            };

            if (nullComparison == null) return null;

            return nullCheck != null
                ? System.Linq.Expressions.Expression.AndAlso(nullCheck, nullComparison)
                : nullComparison;
        }

        // Guard: Add leaf null check for non-null comparisons on reference/nullable types.
        if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
        {
            BinaryExpression leafNotNull = System.Linq.Expressions.Expression.NotEqual(expr, System.Linq.Expressions.Expression.Constant(null, type));
            nullCheck = nullCheck == null ? leafNotNull : System.Linq.Expressions.Expression.AndAlso(nullCheck, leafNotNull);
        }

        // Build: Type-specific comparison expression.
        System.Linq.Expressions.Expression? comparison = type == typeof(string)
            ? BuildStringComparison(expr, constExpr, op, caseSensitive)
            : BuildValueComparison(expr, constExpr, op);

        if (comparison == null) return null;

        // Merge: Short-circuit with accumulated null checks (AndAlso ensures null safety).
        return nullCheck != null
            ? System.Linq.Expressions.Expression.AndAlso(nullCheck, comparison)
            : comparison;
    }

    #endregion Entry Point

    #region String Comparison

    /// <summary>
    /// Builds a string-specific comparison expression (contains, starts-with, ends-with, equality).
    /// </summary>
    internal static System.Linq.Expressions.Expression? BuildStringComparison(
        System.Linq.Expressions.Expression expr,
        ConstantExpression constExpr,
        string op,
        bool caseSensitive)
    {
        MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains),    [typeof(string)])!;
        MethodInfo startsMethod   = typeof(string).GetMethod(nameof(string.StartsWith),  [typeof(string)])!;
        MethodInfo endsMethod     = typeof(string).GetMethod(nameof(string.EndsWith),    [typeof(string)])!;
        MethodInfo toLowerMethod  = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

        System.Linq.Expressions.Expression left  = expr;
        System.Linq.Expressions.Expression right = constExpr;

        if (!caseSensitive)
        {
            // Normalize: Both sides use ToLowerInvariant for consistent case-folding.
            left  = System.Linq.Expressions.Expression.Call(expr, toLowerMethod);
            right = System.Linq.Expressions.Expression.Constant(constExpr.Value?.ToString()?.ToLowerInvariant(), typeof(string));
        }

        try
        {
            return op switch
            {
                FilterOperatorConstant.Tokens.Equal  => System.Linq.Expressions.Expression.Equal(left, right),
                FilterOperatorConstant.Tokens.NotEqual => System.Linq.Expressions.Expression.NotEqual(left, right),
                FilterOperatorConstant.Tokens.Contains  => System.Linq.Expressions.Expression.Call(left, containsMethod, right),
                FilterOperatorConstant.Tokens.NotContains => System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(left, containsMethod, right)),
                FilterOperatorConstant.Tokens.StartsWith  => System.Linq.Expressions.Expression.Call(left, startsMethod, right),
                FilterOperatorConstant.Tokens.NotStartsWith => System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(left, startsMethod, right)),
                FilterOperatorConstant.Tokens.EndsWith  => System.Linq.Expressions.Expression.Call(left, endsMethod, right),
                FilterOperatorConstant.Tokens.NotEndsWith => System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(left, endsMethod, right)),
                _    => throw new NotSupportedException($"Operator '{op}' is not supported for string properties.")
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion String Comparison

    #region Value Comparison

    /// <summary>
    /// Builds a numeric / value-type comparison expression (equality, relational).
    /// </summary>
    internal static BinaryExpression? BuildValueComparison(
        System.Linq.Expressions.Expression expr,
        ConstantExpression constExpr,
        string op)
    {
        try
        {
            return op switch
            {
                FilterOperatorConstant.Tokens.Equal  => System.Linq.Expressions.Expression.Equal(expr, constExpr),
                FilterOperatorConstant.Tokens.NotEqual => System.Linq.Expressions.Expression.NotEqual(expr, constExpr),
                FilterOperatorConstant.Tokens.GreaterThan  => System.Linq.Expressions.Expression.GreaterThan(expr, constExpr),
                FilterOperatorConstant.Tokens.LessThan  => System.Linq.Expressions.Expression.LessThan(expr, constExpr),
                FilterOperatorConstant.Tokens.GreaterThanOrEqual => System.Linq.Expressions.Expression.GreaterThanOrEqual(expr, constExpr),
                FilterOperatorConstant.Tokens.LessThanOrEqual => System.Linq.Expressions.Expression.LessThanOrEqual(expr, constExpr),
                _    => throw new NotSupportedException($"Operator '{op}' is not supported for this value type.")
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion Value Comparison

    #region Constant Parsing

    /// <summary>
    /// Converts a raw string value to a typed <see cref="ConstantExpression"/> for the
    /// given <paramref name="targetType"/>, handling nulls, enums, dates, GUIDs, and booleans.
    /// </summary>
    /// <returns>A typed constant, or <see langword="null"/> if conversion fails.</returns>
    internal static ConstantExpression? ParseConstant(string value, Type targetType)
    {
        if (string.Equals(value, FilterExpressionBuilderConstant.NullSentinel.Value, StringComparison.OrdinalIgnoreCase))
            return System.Linq.Expressions.Expression.Constant(null, targetType);

        if (string.IsNullOrEmpty(value))
        {
            return targetType == typeof(string)
                ? System.Linq.Expressions.Expression.Constant(string.Empty, typeof(string))
                : null;
        }

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType.IsEnum)
            {
                object enumValue = Enum.Parse(underlyingType, value, ignoreCase: true);
                return System.Linq.Expressions.Expression.Constant(enumValue, targetType);
            }

            if (underlyingType == typeof(DateTimeOffset))
            {
                var dto = DateTimeOffset.Parse(
                    value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal);
                return System.Linq.Expressions.Expression.Constant(dto, targetType);
            }

            if (underlyingType == typeof(DateTime))
            {
                var dt = DateTime.Parse(
                    value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal);
                return System.Linq.Expressions.Expression.Constant(dt, targetType);
            }

            if (underlyingType == typeof(Guid))
                return System.Linq.Expressions.Expression.Constant(Guid.Parse(value), targetType);

            if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(value, out bool boolValue))
                    return System.Linq.Expressions.Expression.Constant(boolValue, targetType);

                return value.ToLowerInvariant() switch
                {
                    FilterExpressionBuilderConstant.BooleanAliases.True1 or FilterExpressionBuilderConstant.BooleanAliases.TrueYes or FilterExpressionBuilderConstant.BooleanAliases.TrueY => System.Linq.Expressions.Expression.Constant(true, targetType),
                    FilterExpressionBuilderConstant.BooleanAliases.False0 or FilterExpressionBuilderConstant.BooleanAliases.FalseNo or FilterExpressionBuilderConstant.BooleanAliases.FalseN => System.Linq.Expressions.Expression.Constant(false, targetType),
                    _                   => null
                };
            }

            object converted = Convert.ChangeType(
                value, underlyingType, System.Globalization.CultureInfo.InvariantCulture);
            return System.Linq.Expressions.Expression.Constant(converted, targetType);
        }
        catch
        {
            return null;
        }
    }

    #endregion Constant Parsing
}