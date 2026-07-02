using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterJsonParserTests
{
    #region Happy Path

    [Fact(DisplayName = "Parse: Single condition JSON produces group")]
    public void Parse_SingleCondition_ShouldProduceGroup()
    {
        Result<FilterGroup> result = FilterJsonParser.Parse("""[{"field":"Name","op":"eq","value":"Apple"}]""");
        result.Value.Conditions.Should().HaveCount(1);
        result.Value.Conditions[0].Field.Should().Be("Name");
        result.Value.Conditions[0].Operator.Should().Be(FilterOperator.Equal);
    }

    [Fact(DisplayName = "Parse: Array of conditions combined with AND")]
    public void Parse_ArrayOfConditions_ShouldBeAnd()
    {
        Result<FilterGroup> result = FilterJsonParser.Parse(
            """[{"field":"A","op":"eq","value":"1"},{"field":"B","op":"eq","value":"2"}]""");
        result.Value.Logic.Should().Be(FilterLogic.And);
        result.Value.Conditions.Should().HaveCount(2);
    }

    [Theory(DisplayName = "Parse: Group logic 'or' and default to AND")]
    [InlineData("""[{"logic":"or","conditions":[{"field":"A","op":"eq","value":"1"}]}]""", FilterLogic.Or)]
    [InlineData("""[{"logic":"OR","conditions":[{"field":"A","op":"eq","value":"1"}]}]""", FilterLogic.Or)]
    [InlineData("""[{"conditions":[{"field":"A","op":"eq","value":"1"}]}]""", FilterLogic.And)]
    public void Parse_GroupLogic_ShouldResolveCorrectly(string json, FilterLogic expectedLogic)
    {
        Result<FilterGroup> result = FilterJsonParser.Parse(json);
        result.Value.Groups[0].Logic.Should().Be(expectedLogic);
    }

    [Fact(DisplayName = "Parse: Nested groups supported")]
    public void Parse_NestedGroups_ShouldWork()
    {
        string json = """[{"logic":"and","conditions":[{"field":"A","op":"eq","value":"1"},{"logic":"or","conditions":[{"field":"B","op":"eq","value":"2"}]}]}]""";
        Result<FilterGroup> result = FilterJsonParser.Parse(json);
        result.Value.Groups.Should().HaveCount(1);
        result.Value.Groups[0].Groups.Should().HaveCount(1);
        result.Value.Groups[0].Groups[0].Logic.Should().Be(FilterLogic.Or);
    }

    [Fact(DisplayName = "Parse: Missing value defaults to empty string")]
    public void Parse_MissingValue_ShouldDefaultToEmpty()
    {
        Result<FilterGroup> result = FilterJsonParser.Parse("""[{"field":"Name","op":"eq"}]""");
        result.Value.Conditions[0].Value.Should().Be("");
    }

    [Fact(DisplayName = "Parse: Empty array returns empty group")]
    public void Parse_EmptyArray_ShouldReturnEmptyGroup()
    {
        FilterJsonParser.Parse("[]").Value.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Alias Operators (parameterized)

    [Theory(DisplayName = "Parse: JSON alias operators resolve correctly")]
    [InlineData("contains", FilterOperator.Contains)]
    [InlineData("starts", FilterOperator.StartsWith)]
    [InlineData("ends", FilterOperator.EndsWith)]
    [InlineData("ncontains", FilterOperator.NotContains)]
    [InlineData("nstarts", FilterOperator.NotStartsWith)]
    [InlineData("nends", FilterOperator.NotEndsWith)]
    [InlineData("gt", FilterOperator.GreaterThan)]
    [InlineData("gte", FilterOperator.GreaterThanOrEqual)]
    [InlineData("lt", FilterOperator.LessThan)]
    [InlineData("lte", FilterOperator.LessThanOrEqual)]
    [InlineData("neq", FilterOperator.NotEqual)]
    public void Parse_AliasOperators_ShouldResolve(string opToken, FilterOperator expected)
    {
        string json = "[{\"field\":\"N\",\"op\":\"" + opToken + "\",\"value\":\"x\"}]";
        FilterJsonParser.Parse(json).Value.Conditions[0].Operator.Should().Be(expected);
    }

    #endregion

    #region Error Paths (parameterized)

    [Theory(DisplayName = "Parse: Error paths return correct error codes")]
    [InlineData("{invalid}", "Filter.Json.InvalidStructure")]
    [InlineData("{}", "Filter.Json.InvalidStructure")]
    [InlineData("not-an-array", "Filter.Json.InvalidStructure")]
    [InlineData("""[{"op":"eq","value":"Apple"}]""", "Filter.Field.Missing")]
    [InlineData("""[{"field":"Name","value":"Apple"}]""", "Filter.Operator.Missing")]
    [InlineData("""[{"field":"Name","op":"unknown_op","value":"Apple"}]""", "Filter.Operator.Unknown")]
    [InlineData("""[{"logic":"or"}]""", "Filter.Json.InvalidStructure")]
    public void Parse_ErrorPaths_ShouldReturnCorrectError(string json, string expectedCode)
    {
        Result<FilterGroup> result = FilterJsonParser.Parse(json);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(expectedCode);
    }

    [Fact(DisplayName = "Parse: Invalid JSON with message detail")]
    public void Parse_InvalidJson_ShouldIncludeDetailInMessage()
    {
        Result<FilterGroup> result = FilterJsonParser.Parse("{bad json}");
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Message.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "Parse: Empty/null field name treated as missing field")]
    public void Parse_EmptyField_ShouldReturnMissingField()
    {
        string json = """[{"field":"","op":"eq","value":"Apple"}]""";
        Result<FilterGroup> result = FilterJsonParser.Parse(json);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Filter.Field.Missing");
    }

    #endregion
}
