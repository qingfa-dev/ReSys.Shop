using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelBehaviorTests
{
    [Theory]
    [InlineData("NAME", true)]
    [InlineData("name", true)]
    [InlineData("Age", false)]
    public void HasField_ShouldReturnExpected(string query, bool expected)
    {
        SortModel model = SortModelExtensions.FromString("Name asc").Value;

        model.HasField(query).Should().Be(expected);
    }

    [Theory]
    [InlineData("NAME", false, SortDirection.Descending)]
    [InlineData("Age", true, null)]
    public void ClauseFor_ShouldReturnExpected(string field, bool expectedNull, SortDirection? expectedDirection)
    {
        SortModel model = SortModelExtensions.FromString("Name desc").Value;
        SortClause? clause = model.ClauseFor(field);

        if (expectedNull)
            clause.Should().BeNull();
        else
        {
            clause.Should().NotBeNull();
            clause!.Direction.Should().Be(expectedDirection!.Value);
        }
    }

    [Theory]
    [InlineData(false, "Name")]
    [InlineData(true, "Default")]
    public void ResolveClauses_ShouldReturnExpected(bool useEmpty, string expectedField)
    {
        SortModel model = useEmpty ? SortModel.Empty : SortModelExtensions.FromString("Name asc").Value;
        IReadOnlyList<SortClause> defaults = [new SortClause("Default")];

        IReadOnlyList<SortClause> resolved = model.ResolveClauses(defaults);

        resolved[0].Field.Should().Be(expectedField);
    }
}
