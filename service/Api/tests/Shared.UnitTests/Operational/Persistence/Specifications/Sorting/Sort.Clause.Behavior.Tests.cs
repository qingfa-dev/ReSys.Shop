using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortClauseBehaviorTests
{
    [Theory]
    [InlineData(SortDirection.Ascending, "asc")]
    [InlineData(SortDirection.Descending, "desc")]
    public void DirectionToken_ShouldReturnExpectedToken(SortDirection direction, string expected)
    {
        SortClause clause = new("Field", direction);

        clause.DirectionToken.Should().Be(expected);
    }

    [Theory]
    [InlineData("Field", SortDirection.Ascending, null, "Field asc")]
    [InlineData("Field", SortDirection.Descending, null, "Field desc")]
    [InlineData("Name", SortDirection.Ascending, SortNulls.First, "Name asc nulls first")]
    [InlineData("Name", SortDirection.Descending, SortNulls.Last, "Name desc nulls last")]
    public void ToString_ShouldReturnExpectedRepresentation(
        string field, SortDirection direction, SortNulls? nulls, string expected)
    {
        SortClause clause = new(field, direction, nulls);

        clause.ToString().Should().Be(expected);
    }
}
