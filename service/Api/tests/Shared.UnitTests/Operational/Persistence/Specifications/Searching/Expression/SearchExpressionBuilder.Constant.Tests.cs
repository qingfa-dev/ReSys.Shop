using Shared.Operational.Persistence.Specifications.Searching.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchExpressionBuilderConstantTests
{
    [Fact(DisplayName = "SearchExpressionBuilderConstant: StringContainsMethod is string.Contains(string)")]
    public void StringContainsMethod_ShouldNotBeNull()
    {
        SearchExpressionBuilderConstant.StringContainsMethod.Should().NotBeNull();
    }
}