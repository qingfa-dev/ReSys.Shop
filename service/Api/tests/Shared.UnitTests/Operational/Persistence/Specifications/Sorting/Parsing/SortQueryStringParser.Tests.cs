using Shared.Operational.Persistence.Specifications.Sorting;
using Shared.Operational.Persistence.Specifications.Sorting.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortQueryStringParserTests
{
    [Fact]
    public void ParseEntry_BareField_ShouldReturnAscending()
    {
        Result<SortClause> result = SortQueryStringParser.ParseEntry("Name");

        result.IsSuccess.Should().BeTrue();
        result.Value.Field.Should().Be("Name");
        result.Value.Direction.Should().Be(SortDirection.Ascending);
    }

    [Theory]
    [InlineData("+Name", SortDirection.Ascending)]
    [InlineData("-CreatedAt", SortDirection.Descending)]
    public void ParseEntry_DirectionPrefix_ShouldResolveCorrectly(string entry, SortDirection expected)
    {
        Result<SortClause> result = SortQueryStringParser.ParseEntry(entry);

        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(expected);
    }

    [Theory]
    [InlineData("Name:asc", SortDirection.Ascending)]
    [InlineData("CreatedAt:desc", SortDirection.Descending)]
    public void ParseEntry_ColonSeparated_ShouldResolveCorrectly(string entry, SortDirection expected)
    {
        Result<SortClause> result = SortQueryStringParser.ParseEntry(entry);

        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(expected);
    }

    [Fact]
    public void ParseEntry_PrefixWithNoField_ShouldReturnFailure()
    {
        Result<SortClause> result = SortQueryStringParser.ParseEntry("- ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ParseEntry_InvalidDirection_ShouldReturnFailure()
    {
        Result<SortClause> result = SortQueryStringParser.ParseEntry("Name:sideways");

        result.IsFailure.Should().BeTrue();
    }
}
