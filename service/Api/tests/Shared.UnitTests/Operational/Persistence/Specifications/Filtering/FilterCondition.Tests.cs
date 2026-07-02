using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterConditionTests
{
    [Fact(DisplayName = "FilterCondition: Should construct with all properties")]
    public void ShouldConstruct_WithAllProperties()
    {
        FilterCondition condition = new("Name", FilterOperator.Equal, "Apple");

        condition.Field.Should().Be("Name");
        condition.Operator.Should().Be(FilterOperator.Equal);
        condition.Value.Should().Be("Apple");
    }

    [Fact(DisplayName = "FilterCondition: ToString returns DSL representation")]
    public void ToString_ShouldReturnDslRepresentation()
    {
        FilterCondition condition = new("Name", FilterOperator.Contains, "ap");

        condition.ToString().Should().Be("Name * ap");
    }

    [Fact(DisplayName = "FilterCondition: ToString with greater-than-or-equal operator")]
    public void ToString_ShouldRenderGreaterThanOrEqual()
    {
        FilterCondition condition = new("Age", FilterOperator.GreaterThanOrEqual, "18");

        condition.ToString().Should().Be("Age >= 18");
    }

    [Fact(DisplayName = "FilterCondition: OperatorToken delegates to FilterOperatorMap")]
    public void OperatorToken_ShouldDelegateToFilterOperatorMap()
    {
        FilterCondition condition = new("Name", FilterOperator.NotEqual, "X");

        condition.OperatorToken.Should().Be("!=");
    }

    [Fact(DisplayName = "FilterCondition: IsCaseSensitive true for case-sensitive operators")]
    public void IsCaseSensitive_ShouldBeTrue_ForCaseSensitiveOperators()
    {
        FilterCondition cs = new("Name", FilterOperator.EqualCaseSensitive, "Apple");
        FilterCondition ci = new("Name", FilterOperator.Equal, "Apple");

        cs.IsCaseSensitive.Should().BeTrue();
        ci.IsCaseSensitive.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterCondition: IsNegation true for negation operators")]
    public void IsNegation_ShouldBeTrue_ForNegationOperators()
    {
        FilterCondition neg = new("Name", FilterOperator.NotContains, "x");
        FilterCondition pos = new("Name", FilterOperator.Contains, "x");

        neg.IsNegation.Should().BeTrue();
        pos.IsNegation.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterCondition: IsStringOnly true for string operators")]
    public void IsStringOnly_ShouldBeTrue_ForStringOperators()
    {
        FilterCondition str = new("Name", FilterOperator.StartsWith, "A");
        FilterCondition eq = new("Age", FilterOperator.Equal, "18");

        str.IsStringOnly.Should().BeTrue();
        eq.IsStringOnly.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterCondition: With dot-notation field name")]
    public void ShouldSupportDotNotationFieldName()
    {
        FilterCondition condition = new("Order.Customer.Name", FilterOperator.Equal, "John");

        condition.Field.Should().Be("Order.Customer.Name");
    }
}
