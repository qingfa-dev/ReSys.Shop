using Shared.Performance.Caching.Options.Distributed;

namespace Shared.UnitTests.Performance.Caching.Options.Distributed;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class DistributedCacheConstantTests
{
    [Fact(DisplayName = "Constraints should have expected limits")]
    public void Constraints_ShouldHaveExpectedLimits()
    {
        DistributedCacheConstant.Constraints.DefaultExpirationMinutesMin.Should().Be(1);
    }

    [Fact(DisplayName = "Patterns.ValidTypes should contain expected types")]
    public void Patterns_ValidTypes_ShouldContainExpectedTypes()
    {
        DistributedCacheConstant.Patterns.ValidTypes.Should().BeEquivalentTo(["redis", "sqlserver"]);
    }
}
