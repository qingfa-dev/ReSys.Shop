using Module.Profile.Domain.Preferences;
using Module.Profile.Features.Shared.Profiles.Validators;
using Module.Profile.Features.Shared.Profiles.Models;

namespace Module.UnitTests.Profile.Features.Store.Profile.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileShared")]
public class ProfilePreferencesValidatorTests
{
    private readonly ProfilePreferencesValidator _sut = new();

    [Fact]
    public void ProfilePreferencesValidator_WhenPreferredStyleTooLong_ShouldHaveError()
    {
        var model = new ProfilePreferences
        {
            PreferredStyle = new string('x', UserPreferenceConstant.Constraints.MaxPreferredStyleLength + 1)
        };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PreferredStyle)
            .WithErrorCode(UserPreferencesResult.Failure.StyleTooLong.Code);
    }

    [Fact]
    public void ProfilePreferencesValidator_WhenFavoriteColorsExceedsLimit_ShouldHaveError()
    {
        var model = new ProfilePreferences
        {
            FavoriteColors = Enumerable.Range(0, UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser + 1)
                .Select(i => $"color{i}").ToList()
        };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FavoriteColors)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyFavoriteColors.Code);
    }

    [Fact]
    public void ProfilePreferencesValidator_WhenAllFieldsValid_ShouldPass()
    {
        var model = new ProfilePreferences
        {
            PreferredStyle = "casual",
            PreferredFit = "regular",
            FavoriteColors = ["Black", "Blue"],
            FavoriteCategories = ["Tops", "Bottoms"],
            PreferredBrands = ["Nike", "Adidas"],
            SizeTop = "M",
            SizeBottom = "32",
            ShoeSize = "10"
        };
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProfilePreferencesValidator_WhenAllFieldsNull_ShouldPass()
    {
        var model = new ProfilePreferences();
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
