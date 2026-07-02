using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModeTypeTests
{
    [Theory(DisplayName = "SearchingMode: Enum values match expected integers")]
    [InlineData(SearchMode.Any, 0)]
    [InlineData(SearchMode.All, 1)]
    public void EnumValue_ShouldBeCorrect(SearchMode mode, Int32 expectedValue)
    {
        ((Int32)mode).Should().Be(expectedValue);
    }

    [Fact(DisplayName = "SearchingMode: Should have exactly 2 members")]
    public void ShouldHaveExactlyTwoMembers()
    {
        Enum.GetValues<SearchMode>().Should().HaveCount(2);
    }
}