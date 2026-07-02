using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterModelConstantTests
{
    [Fact(DisplayName = "Defaults: RootLogic is FilterLogic.And")]
    public void Defaults_RootLogic_ShouldBeAnd()
    {
        FilterModelConstant.Defaults.RootLogic.Should().Be(FilterLogic.And);
    }

    [Fact(DisplayName = "Defaults: QueryStringSeparator is ':'")]
    public void Defaults_QueryStringSeparator_ShouldBeColon()
    {
        FilterModelConstant.Defaults.QueryStringSeparator.Should().Be(':');
    }

    [Fact(DisplayName = "Defaults: QueryStringSplitCount is 3")]
    public void Defaults_QueryStringSplitCount_ShouldBeThree()
    {
        FilterModelConstant.Defaults.QueryStringSplitCount.Should().Be(3);
    }

    [Fact(DisplayName = "JsonKeys: All keys match expected values")]
    public void JsonKeys_ShouldMatchExpectedValues()
    {
        FilterModelConstant.JsonKeys.Logic.Should().Be("logic");
        FilterModelConstant.JsonKeys.Conditions.Should().Be("conditions");
        FilterModelConstant.JsonKeys.Field.Should().Be("field");
        FilterModelConstant.JsonKeys.Op.Should().Be("op");
        FilterModelConstant.JsonKeys.Value.Should().Be("value");
        FilterModelConstant.JsonKeys.OrValue.Should().Be("or");
    }

    [Fact(DisplayName = "Cache: Prefix is 'Filter'")]
    public void Cache_Prefix_ShouldBeFilter()
    {
        FilterModelConstant.Cache.Prefix.Should().Be("Filter");
    }

    [Fact(DisplayName = "Expression: ParameterName is 'x'")]
    public void Expression_ParameterName_ShouldBeX()
    {
        FilterModelConstant.Expression.ParameterName.Should().Be("x");
    }

    [Fact(DisplayName = "Dsl: JoinSeparator is ', '")]
    public void Dsl_JoinSeparator_ShouldBeCommaSpace()
    {
        FilterModelConstant.Dsl.JoinSeparator.Should().Be(", ");
    }
}
