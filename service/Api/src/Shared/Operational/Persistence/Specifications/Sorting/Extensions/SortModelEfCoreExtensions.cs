using System.Linq.Expressions;
using System.Reflection;

using Shared.Operational.Persistence.Specifications.Sorting.Expression;

namespace Shared.Operational.Persistence.Specifications.Sorting.Extensions;

/// <summary>
/// IQueryable extension methods for applying a <see cref="SortModel"/> to EF Core queries.
/// </summary>
public static class SortingModelQueryExtensions
{
    /// <summary>
    /// Applies the sort clauses from <paramref name="sortModel"/> to the query
    /// using <c>OrderBy</c>/<c>OrderByDescending</c> for the primary clause
    /// and <c>ThenBy</c>/<c>ThenByDescending</c> for subsequent clauses.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sortModel">The sort model containing ordered clauses.</param>
    /// <param name="defaultClauses">Fallback clauses when <paramref name="sortModel"/> is empty.</param>
    /// <returns>The sorted query, or the original query when no clauses are present.</returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        SortModel sortModel,
        IReadOnlyList<SortClause>? defaultClauses = null) where T : class
    {
        IReadOnlyList<SortClause> clauses = sortModel.ResolveClauses(defaultClauses ?? []);

        if (clauses.Count == 0)
            return query;

        int appliedCount = 0;
        for (int i = 0; i < clauses.Count; i++)
        {
            SortClause clause = clauses[i];
            Expression<Func<T, object>>? keySelector = SortExpressionBuilder.BuildKeySelector<T>(clause);

            if (keySelector is null) continue;

            MethodInfo method = clause.Direction == SortDirection.Descending
                ? (appliedCount == 0 ? SortExpressionBuilderConstant.OrderByDescendingMethod : SortExpressionBuilderConstant.ThenByDescendingMethod)
                : (appliedCount == 0 ? SortExpressionBuilderConstant.OrderByMethod : SortExpressionBuilderConstant.ThenByMethod);

            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), typeof(object));
            query = (IQueryable<T>)genericMethod.Invoke(null, [query, keySelector])!;
            appliedCount++;
        }

        return query;
    }
}
