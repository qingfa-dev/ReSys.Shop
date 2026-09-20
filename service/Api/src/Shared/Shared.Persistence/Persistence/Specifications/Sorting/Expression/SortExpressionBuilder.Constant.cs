using System.Reflection;

namespace Shared.Operational.Persistence.Specifications.Sorting.Expression;

/// <summary>
/// Cached reflection handles for Queryable.OrderBy / OrderByDescending / ThenBy / ThenByDescending.
/// </summary>
/// <remarks>
/// This is a separate class (not a partial of <see cref="SortExpressionBuilder"/>) following the
/// pattern established by <c>SearchExpressionBuilderConstant</c>.
/// </remarks>
internal static class SortExpressionBuilderConstant
{
    public static readonly MethodInfo OrderByMethod =
        GetGenericQueryableMethod(nameof(Queryable.OrderBy), 2);

    public static readonly MethodInfo OrderByDescendingMethod =
        GetGenericQueryableMethod(nameof(Queryable.OrderByDescending), 2);

    public static readonly MethodInfo ThenByMethod =
        GetGenericQueryableMethod(nameof(Queryable.ThenBy), 2);

    public static readonly MethodInfo ThenByDescendingMethod =
        GetGenericQueryableMethod(nameof(Queryable.ThenByDescending), 2);

    /// <summary>
    /// Separator between segments of a dot-notation property path (e.g. "Order.Customer.Name").
    /// </summary>
    public const char NavigationSeparator = '.';

    private static MethodInfo GetGenericQueryableMethod(string name, int parameterCount) =>
        typeof(Queryable).GetMethods()
            .First(m => m.Name == name && m.GetParameters().Length == parameterCount);
}
