using Shared.Operational.Persistence.Specifications.Sorting;
using Shared.Operational.Persistence.Specifications.Sorting.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortDslParserTests
{
    [Fact]
    public void Parse_ValidBareField_ShouldReturnAscending()
    {
        Result<SortClause> result = SortDslParser.Parse("Name");

        result.IsSuccess.Should().BeTrue();
        result.Value.Field.Should().Be("Name");
        result.Value.Direction.Should().Be(SortDirection.Ascending);
    }

    [Theory]
    [InlineData("+Name", SortDirection.Ascending)]
    [InlineData("-CreatedAt", SortDirection.Descending)]
    public void Parse_DirectionPrefix_ShouldResolveCorrectly(string segment, SortDirection expected)
    {
        Result<SortClause> result = SortDslParser.Parse(segment);

        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(expected);
    }

    [Theory]
    [InlineData("Name asc")]
    [InlineData("CreatedAt desc")]
    public void Parse_FieldAndDirection_ShouldResolveCorrectly(string segment)
    {
        Result<SortClause> result = SortDslParser.Parse(segment);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Parse_PrefixWithNoField_ShouldReturnFailure()
    {
        Result<SortClause> result = SortDslParser.Parse("- ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_InvalidDirection_ShouldReturnFailure()
    {
        Result<SortClause> result = SortDslParser.Parse("Name sideways");

        result.IsFailure.Should().BeTrue();
    }
}
