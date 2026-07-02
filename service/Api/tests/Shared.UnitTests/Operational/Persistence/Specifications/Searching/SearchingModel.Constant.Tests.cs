using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelConstantTests
{
    [Fact(DisplayName = "SearchingModel.Default: Fields is empty")]
    public void Default_Fields_ShouldBeEmpty()
    {
        SearchModel.Default.Fields.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchingModel.Default: Mode is SearchingMode.Any")]
    public void Default_Mode_ShouldBeAny()
    {
        SearchModel.Default.Mode.Should().Be(SearchMode.Any);
    }
}