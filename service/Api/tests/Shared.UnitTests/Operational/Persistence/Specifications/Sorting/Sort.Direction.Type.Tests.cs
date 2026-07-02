using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortDirectionTypeTests
{
    [Theory]
    [InlineData(SortDirection.Ascending, 0)]
    [InlineData(SortDirection.Descending, 1)]
    public void EnumValue_ShouldMatchExpected(SortDirection value, int expected)
    {
        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void ShouldHaveExactlyTwoMembers()
    {
        Enum.GetValues<SortDirection>().Length.Should().Be(2);
    }
}
