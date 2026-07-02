using FluentValidation.Results;

using Shared.Performance.Caching.Options.Distributed;

namespace Shared.UnitTests.Performance.Caching.Options.Distributed;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class DistributedCacheSettingValidatorTests
{
    private readonly DistributedCacheSettingValidator _validator = new();

    [Fact(DisplayName = "Valid defaults should pass validation")]
    public void ValidDefaults_ShouldPassValidation()
    {
        DistributedCacheSetting model = new();

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Empty type should fail with TypeRequired")]
    public void Type_Empty_ShouldFailWithTypeRequired()
    {
        DistributedCacheSetting model = new() { Type = string.Empty };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Distributed.Type.Required");
    }

    [Fact(DisplayName = "Invalid type should fail with TypeInvalid")]
    public void Type_Invalid_ShouldFailWithTypeInvalid()
    {
        DistributedCacheSetting model = new() { Type = "mongo" };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Distributed.Type.Invalid");
    }

    [Theory(DisplayName = "Valid type should pass")]
    [InlineData("redis")]
    [InlineData("sqlserver")]
    [InlineData("Redis")]
    [InlineData("SQLServer")]
    public void Type_Valid_ShouldPass(string type)
    {
        DistributedCacheSetting model = new() { Type = type };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "DefaultExpirationMinutes = 0 should fail")]
    public void DefaultExpirationMinutes_Zero_ShouldFail()
    {
        DistributedCacheSetting model = new() { DefaultExpirationMinutes = 0 };

        ValidationResult result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Caching.Distributed.DefaultExpirationMinutes.GreaterThanZero");
    }
}
