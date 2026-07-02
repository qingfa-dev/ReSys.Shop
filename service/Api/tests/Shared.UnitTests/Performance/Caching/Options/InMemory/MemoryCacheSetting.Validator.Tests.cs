using FluentValidation.Results;

using Shared.Performance.Caching.Options.InMemory;

namespace Shared.UnitTests.Performance.Caching.Options.InMemory;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class MemoryCacheSettingValidatorTests
{
    private readonly MemoryCacheSettingValidator _validator = new();

    [Fact(DisplayName = "Valid defaults should pass validation")]
    public void ValidDefaults_ShouldPassValidation()
    {
        MemoryCacheSetting model = new();

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "DefaultExpirationMinutes out of range should fail")]
    [InlineData(0)]
    [InlineData(1441)]
    public void DefaultExpirationMinutes_OutOfRange_ShouldFail(int value)
    {
        MemoryCacheSetting model = new() { DefaultExpirationMinutes = value };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Memory.DefaultExpiration.OutOfRange");
    }

    [Theory(DisplayName = "CompactionPercentage out of range should fail")]
    [InlineData(0)]
    [InlineData(101)]
    public void CompactionPercentage_OutOfRange_ShouldFail(int value)
    {
        MemoryCacheSetting model = new() { CompactionPercentage = value };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Memory.CompactionPercentage.OutOfRange");
    }

    [Fact(DisplayName = "SizeLimitBytes null should pass validation")]
    public void SizeLimitBytes_Null_ShouldPass()
    {
        MemoryCacheSetting model = new() { SizeLimitBytes = null };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "SizeLimitBytes = 0 should fail")]
    public void SizeLimitBytes_Zero_ShouldFail()
    {
        MemoryCacheSetting model = new() { SizeLimitBytes = 0 };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Memory.SizeLimit.OutOfRange");
    }

    [Fact(DisplayName = "SizeLimitBytes valid should pass")]
    public void SizeLimitBytes_Valid_ShouldPass()
    {
        MemoryCacheSetting model = new() { SizeLimitBytes = 1024 };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
