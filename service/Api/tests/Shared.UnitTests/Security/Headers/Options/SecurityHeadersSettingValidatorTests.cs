using FluentValidation.Results;

using Shared.Security.Headers.Options;

namespace Shared.UnitTests.Security.Headers.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "SecurityHeaders")]
public sealed class SecurityHeadersSettingValidatorTests
{
    private readonly SecurityHeadersSettingValidator _validator = new();

    [Fact(DisplayName = "Default options should pass validation")]
    public void Validate_Defaults_ShouldPass()
    {
        var options = new SecurityHeadersSetting();

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Disabled should pass validation")]
    public void Validate_Disabled_ShouldPass()
    {
        var options = new SecurityHeadersSetting { IsEnabled = false };

        ValidationResult result = _validator.Validate(options);

        result.IsValid.Should().BeTrue();
    }
}
