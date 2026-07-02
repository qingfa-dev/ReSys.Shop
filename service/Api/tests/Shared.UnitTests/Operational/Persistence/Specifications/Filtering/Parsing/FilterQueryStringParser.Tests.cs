using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterQueryStringParserTests
{
    [Fact(DisplayName = "Parse: Single triplet produces single condition")]
    public void Parse_SingleTriplet_ShouldProduceSingleCondition()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name:eq:Apple"]);

        result.Value.Conditions.Should().HaveCount(1);
        result.Value.Conditions[0].Field.Should().Be("Name");
        result.Value.Conditions[0].Value.Should().Be("Apple");
        result.Value.Conditions[0].Operator.Should().Be(FilterOperator.Equal);
    }

    [Fact(DisplayName = "Parse: Multiple triplets combined with AND")]
    public void Parse_MultipleTriplets_ShouldBeAnd()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name:eq:Apple", "Age:gt:18"]);

        result.Value.Logic.Should().Be(FilterLogic.And);
        result.Value.Conditions.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Parse: Null input returns empty group")]
    public void Parse_NullInput_ShouldReturnEmptyGroup()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(null);

        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "Parse: Empty sequence returns empty group")]
    public void Parse_EmptySequence_ShouldReturnEmptyGroup()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse([]);

        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "Parse: Whitespace-only entries are filtered out")]
    public void Parse_WhitespaceOnly_ShouldBeFiltered()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["  ", "\t", "Name:eq:Apple"]);

        result.Value.Conditions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Parse: Value containing colon (ISO timestamp) preserved")]
    public void Parse_ValueWithColon_ShouldPreserveTimestamp()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["CreatedAt:gte:2024-01-01T00:00:00Z"]);

        result.Value.Conditions[0].Value.Should().Be("2024-01-01T00:00:00Z");
        result.Value.Conditions[0].Field.Should().Be("CreatedAt");
    }

    [Fact(DisplayName = "Parse: Triplet with 2 parts (missing value) defaults to empty string")]
    public void Parse_TwoPartTriplet_ShouldDefaultToEmptyString()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name:eq"]);

        result.Value.Conditions[0].Value.Should().Be("");
    }

    [Fact(DisplayName = "Parse: Single-part triplet returns InvalidTriplet error")]
    public void Parse_SinglePartTriplet_ShouldReturnError()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name"]);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Filter.QueryString.InvalidTriplet");
    }

    [Fact(DisplayName = "Parse: Unknown operator returns UnknownOperator error")]
    public void Parse_UnknownOperator_ShouldReturnError()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name:unknown:Apple"]);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Filter.Operator.Unknown");
    }

    [Fact(DisplayName = "Parse: Missing field returns MissingField error")]
    public void Parse_MissingField_ShouldReturnError()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse([":eq:Apple"]);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Filter.Field.Missing");
    }

    [Fact(DisplayName = "Parse: Field and value trimmed of whitespace")]
    public void Parse_FieldAndValue_ShouldBeTrimmed()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["  Name  :  eq  :  Apple  "]);

        result.Value.Conditions[0].Field.Should().Be("Name");
        result.Value.Conditions[0].Value.Should().Be("Apple");
    }

    [Fact(DisplayName = "Parse: Operator names case-insensitive")]
    public void Parse_OperatorCaseInsensitive_ShouldWork()
    {
        Result<FilterGroup> result = FilterQueryStringParser.Parse(["Name:EQ:Apple"]);

        result.Value.Conditions[0].Operator.Should().Be(FilterOperator.Equal);
    }

    [Fact(DisplayName = "Parse: JSON alias operators work in query string")]
    public void Parse_AliasOperators_ShouldWork()
    {
        Result<FilterGroup> r1 = FilterQueryStringParser.Parse(["Name:contains:ap"]);
        r1.Value.Conditions[0].Operator.Should().Be(FilterOperator.Contains);

        Result<FilterGroup> r2 = FilterQueryStringParser.Parse(["Name:starts:Ap"]);
        r2.Value.Conditions[0].Operator.Should().Be(FilterOperator.StartsWith);

        Result<FilterGroup> r3 = FilterQueryStringParser.Parse(["Name:ends:le"]);
        r3.Value.Conditions[0].Operator.Should().Be(FilterOperator.EndsWith);
    }
}
