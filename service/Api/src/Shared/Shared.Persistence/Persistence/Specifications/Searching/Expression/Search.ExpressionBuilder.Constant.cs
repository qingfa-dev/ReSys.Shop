using System.Reflection;

namespace Shared.Operational.Persistence.Specifications.Searching.Expression;

internal static class SearchExpressionBuilderConstant
{
    // Contract: Cached reflection handle for string.Contains(string).
    public static readonly MethodInfo StringContainsMethod =
        typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
}