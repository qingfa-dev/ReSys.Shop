using System.Linq.Expressions;

namespace Shared.Operational.Persistence.Specifications.Filtering.Expression;

/// <summary>
/// Visits a <see cref="FilterGroup"/> tree and produces a composed
/// <see cref="Expression"/> predicate suitable for <c>IQueryable&lt;T&gt;.Where()</c>.
/// </summary>
/// <typeparam name="T">The entity type being filtered.</typeparam>
/// <remarks>
/// Each leaf <see cref="FilterCondition"/> is converted to an Expression via
/// <see cref="FilterExpressionBuilder.Build{T}"/>. Sibling conditions/groups are
/// combined with <see cref="Expression.AndAlso"/> or <see cref="Expression.OrElse"/>
/// according to the parent group's <see cref="FilterGroup.Logic"/>.
/// Empty groups and null leaf results are skip-safe.
/// </remarks>
internal static class FilterGroupVisitor<T>
{
    /// <summary>
    /// Builds a predicate <see cref="Expression"/> for the entire
    /// <paramref name="group"/> tree, rooted at <paramref name="param"/>.
    /// </summary>
    /// <param name="group">The root <see cref="FilterGroup"/> to visit.</param>
    /// <param name="param">
    /// The lambda parameter expression (e.g. <c>x</c> in <c>x => ...</c>).
    /// </param>
    /// <returns>
    /// A combined <see cref="Expression"/>, or <see langword="null"/> if the group
    /// is empty or all conditions failed to build.
    /// </returns>
    internal static System.Linq.Expressions.Expression? Build(FilterGroup group, ParameterExpression param)
        => VisitGroup(group, param);

    #region Group Traversal

    private static System.Linq.Expressions.Expression? VisitGroup(FilterGroup group, ParameterExpression param)
    {
        if (group.IsEmpty) return null;

        var expressions = new List<System.Linq.Expressions.Expression>();

        // Leaf conditions.
        foreach (FilterCondition condition in group.Conditions)
        {
            string opToken = FilterOperatorMap.ToDslToken(condition.Operator);
            bool caseSensitive = FilterOperatorMap.IsCaseSensitive(condition.Operator);

            System.Linq.Expressions.Expression? leaf = FilterExpressionBuilder.Build<T>(
                param, condition.Field, opToken, condition.Value, caseSensitive);

            if (leaf is not null)
                expressions.Add(leaf);
        }

        // Nested sub-groups.
        foreach (FilterGroup subGroup in group.Groups)
        {
            System.Linq.Expressions.Expression? nested = VisitGroup(subGroup, param);
            if (nested is not null)
                expressions.Add(nested);
        }

        if (expressions.Count == 0) return null;

        // Combine: Apply the group's logical connective.
        System.Linq.Expressions.Expression combined = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            combined = group.Logic == FilterLogic.And
                ? System.Linq.Expressions.Expression.AndAlso(combined, expressions[i])
                : System.Linq.Expressions.Expression.OrElse(combined, expressions[i]);
        }

        return combined;
    }

    #endregion Group Traversal
}
