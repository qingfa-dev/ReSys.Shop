using FluentValidation.Results;

using Shared.Security.Cors.Options;

namespace Shared.UnitTests.Security.Cors.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Cors")]
public sealed class CorsSettingValidatorTests
{
    private readonly CorsSettingValidator _validator = new();

    [Fact(DisplayName = "Validator should pass with default options")]
    public void Validate_WithDefaultOptions_ShouldSucceed()
    {
        var options = new CorsSetting();

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Validator should pass with valid origins")]
    [InlineData("*")]
    [InlineData("https://example.com")]
    [InlineData("https://shop.example.com")]
    public void Validate_WithValidOrigins_ShouldSucceed(string origin)
    {
        var options = new CorsSetting
        {
            Origins = [origin]
        };

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Validator should fail with null origins")]
    public void Validate_WithNullOrigins_ShouldFail()
    {
        var options = new CorsSetting
        {
            Origins = null!
        };

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .Contain(error =>
                error.ErrorCode ==
                CorsResult.Errors.OriginsNull.Code);
    }

    [Theory(DisplayName = "Validator should fail when wildcard is mixed with explicit origins")]
    [InlineData("*", "https://example.com")]
    [InlineData("*", "https://shop.example.com")]
    [InlineData("*", "https://api.example.com")]
    public void Validate_WithAmbiguousOrigins_ShouldFail(
        string wildcard,
        string explicitOrigin)
    {
        var options = new CorsSetting
        {
            Origins = [wildcard, explicitOrigin]
        };

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .Contain(error =>
                error.ErrorCode ==
                CorsResult.Errors.AmbiguousOrigin.Code);
    }
}