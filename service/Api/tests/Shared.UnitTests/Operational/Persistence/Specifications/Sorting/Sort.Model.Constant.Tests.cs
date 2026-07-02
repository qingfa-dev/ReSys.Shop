using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelConstantTests
{
    [Fact]
    public void Default_Clauses_ShouldBeEmpty()
    {
        SortModel.Default.Clauses.Should().BeEmpty();
    }

    [Fact]
    public void Empty_ShouldHaveNoClauses()
    {
        SortModel.Empty.Clauses.Should().BeEmpty();
        SortModel.Empty.IsEmpty.Should().BeTrue();
    }
}
