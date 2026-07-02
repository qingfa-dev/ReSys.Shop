using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortClauseTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldUseDefaults()
    {
        SortClause clause = new("Name");

        clause.Field.Should().Be("Name");
        clause.Direction.Should().Be(SortDirection.Ascending);
        clause.Nulls.Should().BeNull();
    }

    [Theory]
    [InlineData("Name", "Name", SortDirection.Descending, SortNulls.First, true)]
    [InlineData("CreatedAt", "CreatedAt", SortDirection.Descending, SortNulls.First, true)]
    [InlineData("Name", "Age", SortDirection.Ascending, null, false)]
    public void Equality_ShouldWork(
        string fieldA, string fieldB, SortDirection dir, SortNulls? nulls, bool expectEqual)
    {
        SortClause a = new(fieldA, dir, nulls);
        SortClause b = new(fieldB, dir, nulls);

        if (expectEqual)
            a.Should().Be(b);
        else
            a.Should().NotBe(b);
    }
}
