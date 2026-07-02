using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortNullsTypeTests
{
    [Theory]
    [InlineData(SortNulls.First, 0)]
    [InlineData(SortNulls.Last, 1)]
    public void EnumValue_ShouldMatchExpected(SortNulls value, int expected)
    {
        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void ShouldHaveExactlyTwoMembers()
    {
        Enum.GetValues<SortNulls>().Length.Should().Be(2);
    }
}
