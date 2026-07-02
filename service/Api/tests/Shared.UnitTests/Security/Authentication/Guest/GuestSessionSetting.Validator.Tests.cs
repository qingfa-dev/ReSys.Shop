using FluentValidation.TestHelper;

using Shared.Security.Authentication.Guest.Options;

namespace Shared.UnitTests.Security.Authentication.Guest;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "GuestSession")]
public sealed class GuestSessionSettingValidatorTests
{
    private readonly GuestSessionSettingValidator _sut = new();

    public static TheoryData<
        Action<GuestSessionSetting>,
        string> InvalidCases =>
        new()
        {
            {
                x => x.CookieName = string.Empty,
                GuestSessionSettingResult.Failure.CookieNameRequired.Code
            },
            {
                x => x.CookieName = new string('x', 257),
                GuestSessionSettingResult.Failure.CookieNameInvalid.Code
            },
            {
                x => x.CookieSameSite = "Invalid",
                GuestSessionSettingResult.Failure.CookieSameSiteInvalid.Code
            },
            {
                x => x.CookieSecurePolicy = "Bogus",
                GuestSessionSettingResult.Failure.CookieSecurePolicyInvalid.Code
            },
            {
                x => x.ExpirationInDays = 0,
                GuestSessionSettingResult.Failure.ExpirationInDaysInvalid.Code
            },
            {
                x => x.ExpirationInDays = 3651,
                GuestSessionSettingResult.Failure.ExpirationInDaysInvalid.Code
            }
        };

    [Fact(DisplayName = "Validate should pass for default options")]
    public void Validate_WithDefaultOptions_ShouldPass()
    {
        GuestSessionSetting options = new();

        TestValidationResult<GuestSessionSetting> result = _sut.TestValidate(options);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Validate_WithInvalidValue_ShouldFail(
        Action<GuestSessionSetting> setup,
        string expectedErrorCode)
    {
        GuestSessionSetting options = new();
        setup(options);

        TestValidationResult<GuestSessionSetting> result = _sut.TestValidate(options);

        result.Errors.Should().Contain(e => e.ErrorCode == expectedErrorCode);
    }
}
