using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterLogicTypeTests
{
    [Fact(DisplayName = "FilterLogic: And = 0")]
    public void And_ShouldBeZero()
    {
        ((int)FilterLogic.And).Should().Be(0);
    }

    [Fact(DisplayName = "FilterLogic: Or = 1")]
    public void Or_ShouldBeOne()
    {
        ((int)FilterLogic.Or).Should().Be(1);
    }

    [Fact(DisplayName = "FilterLogic: Should have exactly 2 members")]
    public void ShouldHaveExactlyTwoMembers()
    {
        Enum.GetValues<FilterLogic>().Should().HaveCount(2);
    }
}
