using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Searching.Expression;

namespace Shared.Operational.Persistence.Specifications.Searching.Extensions;

// Boundary: IQueryable extensions for applying search models to EF Core queries.
public static class SearchModelQueryExtensions
{
    // Contract: Applies a SearchingModel as a Where clause. Returns query unchanged for empty models.
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        SearchModel model,
        IReadOnlyList<string>? defaultFields = null) where T : class
    {
        if (model.IsEmpty)
        {
            return query;
        }

        Expression<Func<T, bool>> predicate =
            SearchExpressionBuilder.Build<T>(model, defaultFields);

        return query.Where(predicate);
    }
}