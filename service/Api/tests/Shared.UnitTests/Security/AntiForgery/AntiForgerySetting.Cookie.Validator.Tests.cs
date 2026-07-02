using FluentValidation.TestHelper;

using Shared.Security.AntiForgery.Options;

namespace Shared.UnitTests.Security.AntiForgery;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Storage")]
public sealed class AntiForgerySettingCookieValidatorTests
{
    private readonly AntiForgerySettingValidator _sut = new();

    public static TheoryData<
        Action<AntiForgerySetting>,
        string> InvalidCases =>
        new()
        {
            {
                x => x.CookieName = string.Empty,
                AntiForgerySettingResult.Failure.CookieNameRequired.Code
            },
            {
                x => x.CookieName = new string('x', 257),
                AntiForgerySettingResult.Failure.CookieNameInvalid.Code
            },
            {
                x => x.CookieSameSite = "Invalid",
                AntiForgerySettingResult.Failure.CookieSameSiteInvalid.Code
            },
            {
                x => x.CookieSecurePolicy = "Bogus",
                AntiForgerySettingResult.Failure.CookieSecurePolicyInvalid.Code
            },
            {
                x => { x.CookieMaxAgeMinutes = -1; },
                AntiForgerySettingResult.Failure.CookieMaxAgeMinutesInvalid.Code
            }
        };

    [Fact(DisplayName = "Validate should pass for default cookie options")]
    public void Validate_WithDefaultCookieOptions_ShouldPass()
    {
        AntiForgerySetting options = new();

        TestValidationResult<AntiForgerySetting> result = _sut.TestValidate(options);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Validate_WithInvalidCookieValue_ShouldFail(
        Action<AntiForgerySetting> setup,
        string expectedErrorCode)
    {
        AntiForgerySetting options = new();
        setup(options);

        TestValidationResult<AntiForgerySetting> result = _sut.TestValidate(options);

        result.Errors.Should().Contain(e => e.ErrorCode == expectedErrorCode);
    }
}
