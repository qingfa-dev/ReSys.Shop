using Module.Profile.Features.Shared.Profiles.Validators;
using Module.Profile.Features.Shared.Profiles.Models;

namespace Module.UnitTests.Profile.Features.Store.Profile.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileShared")]
public class ProfileNotificationPreferencesValidatorTests
{
    private readonly ProfileNotificationPreferencesValidator _sut = new();

    [Fact]
    public void ProfileNotificationPreferencesValidator_WhenDefault_ShouldPass()
    {
        var model = new ProfileNotificationPreferences();
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProfileNotificationPreferencesValidator_WhenAllFalse_ShouldPass()
    {
        var model = new ProfileNotificationPreferences
        {
            EnableSms = false,
            EnableEmail = false,
            EnableNewsfeeds = false
        };
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
