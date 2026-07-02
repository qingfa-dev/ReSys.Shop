using System.Reflection;

using Shared.Operational.Persistence.Specifications.Sorting.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortExpressionBuilderConstantTests
{
    [Theory]
    [InlineData(nameof(SortExpressionBuilderConstant.OrderByMethod))]
    [InlineData(nameof(SortExpressionBuilderConstant.OrderByDescendingMethod))]
    [InlineData(nameof(SortExpressionBuilderConstant.ThenByMethod))]
    [InlineData(nameof(SortExpressionBuilderConstant.ThenByDescendingMethod))]
    public void MethodInfo_ShouldNotBeNull(string methodName)
    {
        FieldInfo field = typeof(SortExpressionBuilderConstant)
            .GetField(methodName, BindingFlags.Public | BindingFlags.Static)!;

        ((MethodInfo)field.GetValue(null)!).Should().NotBeNull();
    }
}
