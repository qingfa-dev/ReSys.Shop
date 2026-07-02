using Shared.Performance.Caching.Options.InMemory;

namespace Shared.UnitTests.Performance.Caching.Options.InMemory;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class MemoryCacheConstantTests
{
    [Fact(DisplayName = "Constraints should have expected limits")]
    public void Constraints_ShouldHaveExpectedLimits()
    {
        MemoryCacheConstants.Constraints.DefaultExpirationMinutesMin.Should().Be(1);
        MemoryCacheConstants.Constraints.DefaultExpirationMinutesMax.Should().Be(1440);
        MemoryCacheConstants.Constraints.CompactionPercentageMin.Should().Be(1);
        MemoryCacheConstants.Constraints.CompactionPercentageMax.Should().Be(100);
        MemoryCacheConstants.Constraints.SizeLimitBytesMin.Should().Be(1);
        MemoryCacheConstants.Constraints.SizeLimitBytesMax.Should().Be(long.MaxValue);
    }

    [Fact(DisplayName = "Defaults should have expected values")]
    public void Defaults_ShouldHaveExpectedValues()
    {
        MemoryCacheConstants.Defaults.Enabled.Should().BeTrue();
        MemoryCacheConstants.Defaults.DefaultExpirationMinutes.Should().Be(30);
        MemoryCacheConstants.Defaults.CompactionPercentage.Should().Be(25);
        MemoryCacheConstants.Defaults.SizeLimitBytes.Should().Be(100 * 1024 * 1024);
    }
}
