using FluentValidation.Results;

using Shared.Performance.Caching.Options.Hybrid;

namespace Shared.UnitTests.Performance.Caching.Options.Hybrid;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class HybridCacheSettingValidatorTests
{
    private readonly HybridCacheSettingValidator _validator = new();

    [Fact(DisplayName = "Valid defaults should pass validation")]
    public void ValidDefaults_ShouldPassValidation()
    {
        HybridCacheSetting model = new();

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "DefaultExpirationMinutes out of range should fail")]
    [InlineData(0)]
    [InlineData(1441)]
    public void DefaultExpirationMinutes_OutOfRange_ShouldFail(int value)
    {
        HybridCacheSetting model = new() { DefaultExpirationMinutes = value };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Hybrid.DefaultExpiration.OutOfRange");
    }

    [Theory(DisplayName = "MaximumPayloadBytes out of range should fail")]
    [InlineData(0)]
    [InlineData(10 * 1024 * 1024 + 1)]
    public void MaximumPayloadBytes_OutOfRange_ShouldFail(long value)
    {
        HybridCacheSetting model = new() { MaximumPayloadBytes = value };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Hybrid.MaximumPayloadBytes.OutOfRange");
    }

    [Theory(DisplayName = "MaximumKeyLength out of range should fail")]
    [InlineData(0)]
    [InlineData(2049)]
    public void MaximumKeyLength_OutOfRange_ShouldFail(int value)
    {
        HybridCacheSetting model = new() { MaximumKeyLength = value };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Hybrid.MaximumKeyLength.OutOfRange");
    }
}
