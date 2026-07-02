using Shared.Performance.Caching.Options.Hybrid;

namespace Shared.UnitTests.Performance.Caching.Options.Hybrid;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class HybridCacheConstantTests
{
    [Fact(DisplayName = "Constraints should have expected limits")]
    public void Constraints_ShouldHaveExpectedLimits()
    {
        HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMin.Should().Be(1);
        HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMax.Should().Be(1440);
        HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMin.Should().Be(1);
        HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMax.Should().Be(10 * 1024 * 1024);
        HybridCacheSettingConstant.Constraints.MaximumKeyLengthMin.Should().Be(1);
        HybridCacheSettingConstant.Constraints.MaximumKeyLengthMax.Should().Be(2048);
    }

    [Fact(DisplayName = "Defaults should have expected values")]
    public void Defaults_ShouldHaveExpectedValues()
    {
        HybridCacheSettingConstant.Defaults.Enabled.Should().BeTrue();
        HybridCacheSettingConstant.Defaults.DefaultExpirationMinutes.Should().Be(30);
        HybridCacheSettingConstant.Defaults.MaximumPayloadBytes.Should().Be(1024 * 1024);
        HybridCacheSettingConstant.Defaults.MaximumKeyLength.Should().Be(1024);
    }
}
