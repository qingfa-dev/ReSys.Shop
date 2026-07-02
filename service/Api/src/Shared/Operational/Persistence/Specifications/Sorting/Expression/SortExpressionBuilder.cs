using Shared.Operational.Persistence.Specifications.Helpers;

using LinqExpr = System.Linq.Expressions.Expression;

namespace Shared.Operational.Persistence.Specifications.Sorting.Expression;

/// <summary>
/// Builds <see cref="System.Linq.Expressions.Expression{TDelegate}"/> key selectors
/// for applying <see cref="SortModel"/> to <see cref="IQueryable{T}"/> queries.
/// </summary>
internal static class SortExpressionBuilder
{
    /// <summary>
    /// Builds a key selector expression for a single sort clause.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="clause">The sort clause defining the field and direction.</param>
    /// <returns>A lambda expression <c>x => x.Field</c> suitable for OrderBy/ThenBy, or null if the property path cannot be resolved.</returns>
    public static System.Linq.Expressions.Expression<Func<T, object>>? BuildKeySelector<T>(SortClause clause) where T : class
    {
        return BuildKeySelector<T>(clause.Field);
    }

    /// <summary>
    /// Builds a key selector expression for a property path.
    /// Supports dot-notation for navigation properties (e.g. <c>"Order.Customer.Name"</c>).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyPath">The property name or dot-separated navigation path.</param>
    /// <returns>A lambda expression <c>x => x.Property</c> suitable for OrderBy/ThenBy, or null if the property path cannot be resolved on type <typeparamref name="T"/>.</returns>
    public static System.Linq.Expressions.Expression<Func<T, object>>? BuildKeySelector<T>(string propertyPath) where T : class
    {
        System.Linq.Expressions.ParameterExpression parameter = LinqExpr.Parameter(typeof(T), "x");
        LinqExpr? expr = null;
        Type currentType = typeof(T);

        foreach (string segment in propertyPath.Split(SortExpressionBuilderConstant.NavigationSeparator))
        {
            System.Reflection.PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(currentType, segment);

            if (property is null)
            {
                return null;
            }

            LinqExpr memberExpr = LinqExpr.Property(expr ?? (LinqExpr)parameter, property);
            expr = memberExpr;
            currentType = property.PropertyType;
        }

        if (expr is null) return null;

        // Box value types to object so the lambda returns object.
        System.Linq.Expressions.UnaryExpression converted = LinqExpr.Convert(expr, typeof(object));

        return LinqExpr.Lambda<Func<T, object>>(converted, parameter);
    }
}
