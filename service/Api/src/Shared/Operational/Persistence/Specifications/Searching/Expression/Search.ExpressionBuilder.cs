using Shared.Operational.Persistence.Specifications.Helpers;

using LinqExpr = System.Linq.Expressions.Expression;

namespace Shared.Operational.Persistence.Specifications.Searching.Expression;

internal static class SearchExpressionBuilder
{
    // AgentHint: Builds Expression<Func<T, bool>> that checks if any/all searchable fields contain the term.
    // Mode.Any → OR across fields. Mode.All → AND across fields. Empty model → x => true.
    public static System.Linq.Expressions.Expression<Func<T, bool>> Build<T>(
        SearchModel model,
        IReadOnlyList<string>? defaultFields = null) where T : class
    {
        if (model.IsEmpty)
        {
            System.Linq.Expressions.Expression<Func<T, bool>> trueExpression = static _ => true;
            return trueExpression;
        }

        IReadOnlyList<string> fields = model.ResolveFields(defaultFields ?? []);

        if (fields.Count == 0)
        {
            System.Linq.Expressions.Expression<Func<T, bool>> trueExpression = static _ => true;
            return trueExpression;
        }

        System.Linq.Expressions.ParameterExpression parameter = LinqExpr.Parameter(typeof(T), "x");
        string searchValue = model.Term.CaseSensitive
            ? model.Term.Value
            : model.Term.Value.ToLowerInvariant();

        System.Linq.Expressions.Expression? body = null;

        foreach (string field in fields)
        {
            System.Reflection.PropertyInfo? propertyInfo = QueryHelper.GetPropertyCaseInsensitive(typeof(T), field);
            if (propertyInfo is null) continue;

            System.Linq.Expressions.MemberExpression property = LinqExpr.Property(parameter, propertyInfo);
            System.Linq.Expressions.Expression propertyValue = property;

            if (propertyInfo.PropertyType != typeof(string))
            {
                propertyValue = LinqExpr.Call(propertyValue, nameof(object.ToString), Type.EmptyTypes);
            }

            if (!model.Term.CaseSensitive)
            {
                propertyValue = LinqExpr.Call(propertyValue, nameof(string.ToLower), Type.EmptyTypes);
            }

            System.Linq.Expressions.MethodCallExpression containsCall = LinqExpr.Call(
                propertyValue,
                SearchExpressionBuilderConstant.StringContainsMethod,
                LinqExpr.Constant(searchValue));

            if (body is null)
            {
                body = containsCall;
            }
            else if (model.Mode == SearchMode.Any)
            {
                body = LinqExpr.OrElse(body, containsCall);
            }
            else
            {
                body = LinqExpr.AndAlso(body, containsCall);
            }
        }

        if (body is null)
        {
            System.Linq.Expressions.Expression<Func<T, bool>> trueExpression = static _ => true;
            return trueExpression;
        }

        return LinqExpr.Lambda<Func<T, bool>>(body, parameter);
    }
}