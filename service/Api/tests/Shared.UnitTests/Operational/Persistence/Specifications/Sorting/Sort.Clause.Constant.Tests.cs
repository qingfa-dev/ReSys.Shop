using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortClauseConstantTests
{
    [Fact]
    public void DefaultDirection_ShouldBeAscending()
    {
        SortClause.Constant.DefaultDirection.Should().Be(SortDirection.Ascending);
    }

    [Fact]
    public void DefaultNulls_ShouldBeNull()
    {
        SortClause.Constant.DefaultNulls.Should().BeNull();
    }
}
