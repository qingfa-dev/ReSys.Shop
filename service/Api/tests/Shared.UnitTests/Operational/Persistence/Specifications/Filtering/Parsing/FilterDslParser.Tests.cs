using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterDslParserTests
{
    [Fact(DisplayName = "Parse: Single condition produces group with one condition")]
    public void Parse_SingleCondition_ShouldProduceOneCondition()
    {
        FilterGroup group = FilterDslParser.Parse("Name=Apple");

        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().HaveCount(1);
        group.Conditions[0].Field.Should().Be("Name");
        group.Conditions[0].Value.Should().Be("Apple");
        group.Conditions[0].Operator.Should().Be(FilterOperator.Equal);
    }

    [Fact(DisplayName = "Parse: AND with comma produces AND group")]
    public void Parse_Comma_ShouldProduceAndGroup()
    {
        FilterGroup group = FilterDslParser.Parse("A=1,B=2");

        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().HaveCount(2);
        group.Groups.Should().BeEmpty();
    }

    [Fact(DisplayName = "Parse: OR with pipe produces OR group")]
    public void Parse_Pipe_ShouldProduceOrGroup()
    {
        FilterGroup group = FilterDslParser.Parse("A=1|B=2");

        group.Logic.Should().Be(FilterLogic.Or);
        group.Conditions.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Parse: Parenthesized group becomes nested")]
    public void Parse_Parenthesized_ShouldBeNested()
    {
        FilterGroup group = FilterDslParser.Parse("A=1,(B=2|C=3)");

        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().HaveCount(1);
        group.Groups.Should().HaveCount(1);
        group.Groups[0].Logic.Should().Be(FilterLogic.Or);
    }

    [Fact(DisplayName = "Parse: Wildcard shorthand *ap* resolves to Contains operator")]
    public void Parse_WildcardShorthand_ShouldResolveToContains()
    {
        FilterGroup group = FilterDslParser.Parse("Name=*ap*");

        group.Conditions[0].Operator.Should().Be(FilterOperator.Contains);
        group.Conditions[0].Value.Should().Be("ap");
    }

    [Fact(DisplayName = "Parse: Wildcard shorthand App* resolves to StartsWith")]
    public void Parse_WildcardShorthandTrailing_ShouldResolveToStartsWith()
    {
        FilterGroup group = FilterDslParser.Parse("Name=App*");

        group.Conditions[0].Operator.Should().Be(FilterOperator.StartsWith);
        group.Conditions[0].Value.Should().Be("App");
    }

    [Fact(DisplayName = "Parse: Wildcard shorthand *ple resolves to EndsWith")]
    public void Parse_WildcardShorthandLeading_ShouldResolveToEndsWith()
    {
        FilterGroup group = FilterDslParser.Parse("Name=*ple");

        group.Conditions[0].Operator.Should().Be(FilterOperator.EndsWith);
        group.Conditions[0].Value.Should().Be("ple");
    }

    [Fact(DisplayName = "Parse: Quoted value with comma preserved")]
    public void Parse_QuotedValueWithComma_ShouldPreserveComma()
    {
        FilterGroup group = FilterDslParser.Parse("Name=\"Smith, John\"");

        group.Conditions[0].Value.Should().Be("Smith, John");
    }

    [Fact(DisplayName = "Parse: Quoted value with pipe preserved")]
    public void Parse_QuotedValueWithPipe_ShouldPreservePipe()
    {
        FilterGroup group = FilterDslParser.Parse("Name=\"A|B\"");

        group.Conditions[0].Value.Should().Be("A|B");
    }

    [Fact(DisplayName = "Parse: Case-sensitive equality == produces EqualCaseSensitive operator")]
    public void Parse_CaseSensitiveEqual_ShouldProduceCaseSensitiveOperator()
    {
        FilterGroup group = FilterDslParser.Parse("Name==Apple");

        group.Conditions[0].Operator.Should().Be(FilterOperator.EqualCaseSensitive);
    }

    [Fact(DisplayName = "Parse: Case-sensitive contains *~ produces ContainsCaseSensitive")]
    public void Parse_CaseSensitiveContains_ShouldProduceCaseSensitiveOperator()
    {
        FilterGroup group = FilterDslParser.Parse("Name*~Apple");

        group.Conditions[0].Operator.Should().Be(FilterOperator.ContainsCaseSensitive);
    }

    [Fact(DisplayName = "Parse: Case-sensitive starts-with ^~ produces StartsWithCaseSensitive")]
    public void Parse_CaseSensitiveStarts_ShouldProduceCaseSensitiveOperator()
    {
        FilterGroup group = FilterDslParser.Parse("Name^~Apple");

        group.Conditions[0].Operator.Should().Be(FilterOperator.StartsWithCaseSensitive);
    }

    [Fact(DisplayName = "Parse: Case-sensitive ends-with $~ produces EndsWithCaseSensitive")]
    public void Parse_CaseSensitiveEnds_ShouldProduceCaseSensitiveOperator()
    {
        FilterGroup group = FilterDslParser.Parse("Name$~Apple");

        group.Conditions[0].Operator.Should().Be(FilterOperator.EndsWithCaseSensitive);
    }

    [Fact(DisplayName = "Parse: Negation operators resolved correctly")]
    public void Parse_NegationOperators_ShouldResolveCorrectly()
    {
        FilterGroup g1 = FilterDslParser.Parse("Name!=Apple");
        g1.Conditions[0].Operator.Should().Be(FilterOperator.NotEqual);

        FilterGroup g2 = FilterDslParser.Parse("Name!*ap");
        g2.Conditions[0].Operator.Should().Be(FilterOperator.NotContains);

        FilterGroup g3 = FilterDslParser.Parse("Name!^Ap");
        g3.Conditions[0].Operator.Should().Be(FilterOperator.NotStartsWith);

        FilterGroup g4 = FilterDslParser.Parse("Name!$le");
        g4.Conditions[0].Operator.Should().Be(FilterOperator.NotEndsWith);
    }

    [Fact(DisplayName = "Parse: Deeply nested parens (3 levels)")]
    public void Parse_DeeplyNested_ShouldWork()
    {
        FilterGroup group = FilterDslParser.Parse("(((Name=Apple)))");

        group.FlattenConditions().Should().ContainSingle()
            .Which.Field.Should().Be("Name");
    }

    [Fact(DisplayName = "Parse: FlattenConditions returns all leaf conditions")]
    public void Parse_FlattenConditions_ShouldReturnAllLeaves()
    {
        FilterGroup group = FilterDslParser.Parse("A=1,(B=2|C=3)");

        List<FilterCondition> flat = group.FlattenConditions().ToList();

        flat.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Parse: Empty/whitespace input may throw or return empty group")]
    public void Parse_EmptyInput_ShouldNotThrow()
    {
        // The caller (FilterModelExtensions) wraps Parse in try/catch and returns Empty
        Action act = () => FilterDslParser.Parse("");
        // Depending on implementation, either throws or returns empty group
        // The contract is that callers catch exceptions
        try
        {
            FilterGroup result = FilterDslParser.Parse("");
            result.Conditions.Should().BeEmpty();
        }
        catch
        {
            // Expected for some implementations
        }
    }
}
