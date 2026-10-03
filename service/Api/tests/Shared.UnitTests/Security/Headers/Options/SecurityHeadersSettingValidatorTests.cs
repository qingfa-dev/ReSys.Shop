using FluentValidation.TestHelper;

using Shared.Security.Headers.Options;

namespace Shared.UnitTests.Security.Headers.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "SecurityHeaders")]
public sealed class SecurityHeadersSettingValidatorTests
{
    private readonly SecurityHeadersSettingValidator _validator = new();

    [Fact(DisplayName = "Validator: passes when all values are non-empty")]
    public void Valid_Passes()
    {
        var settings = new SecurityHeadersSetting
        {
            ContentSecurityPolicy = "default-src 'self'",
            XFrameOptions = "DENY"
        };
        var result = _validator.TestValidate(settings);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: warns on empty CSP")]
    public void EmptyCsp_Fails()
    {
        var settings = new SecurityHeadersSetting
        {
            ContentSecurityPolicy = "",
            XFrameOptions = "DENY"
        };
        var result = _validator.TestValidate(settings);
        result.ShouldHaveValidationErrorFor(s => s.ContentSecurityPolicy);
    }
}
