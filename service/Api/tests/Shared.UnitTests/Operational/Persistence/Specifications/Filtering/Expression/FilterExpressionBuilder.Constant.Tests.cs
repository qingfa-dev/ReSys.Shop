using Shared.Operational.Persistence.Specifications.Filtering.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterExpressionBuilderConstantTests
{
    [Fact(DisplayName = "NullSentinel: Value is 'null'")]
    public void NullSentinel_Value_ShouldBeNull()
    {
        FilterExpressionBuilderConstant.NullSentinel.Value.Should().Be("null");
    }

    [Fact(DisplayName = "BooleanAliases: True constants are '1', 'yes', 'y'")]
    public void BooleanAliases_True_ShouldMatch()
    {
        FilterExpressionBuilderConstant.BooleanAliases.True1.Should().Be("1");
        FilterExpressionBuilderConstant.BooleanAliases.TrueYes.Should().Be("yes");
        FilterExpressionBuilderConstant.BooleanAliases.TrueY.Should().Be("y");
    }

    [Fact(DisplayName = "BooleanAliases: False constants are '0', 'no', 'n'")]
    public void BooleanAliases_False_ShouldMatch()
    {
        FilterExpressionBuilderConstant.BooleanAliases.False0.Should().Be("0");
        FilterExpressionBuilderConstant.BooleanAliases.FalseNo.Should().Be("no");
        FilterExpressionBuilderConstant.BooleanAliases.FalseN.Should().Be("n");
    }

    [Fact(DisplayName = "Navigation: Separator is '.'")]
    public void Navigation_Separator_ShouldBeDot()
    {
        FilterExpressionBuilderConstant.Navigation.Separator.Should().Be('.');
    }
}
