using Module.Profile.Domain.Preferences;

namespace Module.UnitTests.Profile.Domain.Preferences;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserPreferencesValidation")]
public class UserPreferencesValidationTests
{
    private sealed class PreferredStyleTestModel
    {
        public string? PreferredStyle { get; set; }
    }

    private sealed class PreferredStyleValidator : AbstractValidator<PreferredStyleTestModel>
    {
        public PreferredStyleValidator()
        {
            RuleFor(x => x.PreferredStyle).ApplyPreferredStyleRules();
        }
    }

    [Fact]
    public void ApplyPreferredStyleRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new PreferredStyleValidator();
        var longStyle = new string('a', UserPreferenceConstant.Constraints.MaxPreferredStyleLength + 1);
        var result = validator.TestValidate(new PreferredStyleTestModel { PreferredStyle = longStyle });
        result.ShouldHaveValidationErrorFor(x => x.PreferredStyle)
            .WithErrorCode(UserPreferencesResult.Failure.StyleTooLong.Code);
    }

    [Theory]
    [InlineData("casual!!")]
    [InlineData("formal@home")]
    [InlineData("sporty#1")]
    public void ApplyPreferredStyleRules_WhenInvalidCharacters_ShouldHaveError(string style)
    {
        var validator = new PreferredStyleValidator();
        var result = validator.TestValidate(new PreferredStyleTestModel { PreferredStyle = style });
        result.ShouldHaveValidationErrorFor(x => x.PreferredStyle)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidStyle.Code);
    }

    [Theory]
    [InlineData("casual")]
    [InlineData("formal")]
    [InlineData("sporty")]
    [InlineData("street-wear")]
    [InlineData("high fashion")]
    public void ApplyPreferredStyleRules_WhenValid_ShouldPass(string style)
    {
        var validator = new PreferredStyleValidator();
        var result = validator.TestValidate(new PreferredStyleTestModel { PreferredStyle = style });
        result.ShouldNotHaveValidationErrorFor(x => x.PreferredStyle);
    }

    private sealed class PreferredFitTestModel
    {
        public string? PreferredFit { get; set; }
    }

    private sealed class PreferredFitValidator : AbstractValidator<PreferredFitTestModel>
    {
        public PreferredFitValidator()
        {
            RuleFor(x => x.PreferredFit).ApplyPreferredFitRules();
        }
    }

    [Fact]
    public void ApplyPreferredFitRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new PreferredFitValidator();
        var longFit = new string('a', UserPreferenceConstant.Constraints.MaxPreferredFitLength + 1);
        var result = validator.TestValidate(new PreferredFitTestModel { PreferredFit = longFit });
        result.ShouldHaveValidationErrorFor(x => x.PreferredFit)
            .WithErrorCode(UserPreferencesResult.Failure.FitTooLong.Code);
    }

    [Theory]
    [InlineData("slim!")]
    [InlineData("regular.fit")]
    public void ApplyPreferredFitRules_WhenInvalidCharacters_ShouldHaveError(string fit)
    {
        var validator = new PreferredFitValidator();
        var result = validator.TestValidate(new PreferredFitTestModel { PreferredFit = fit });
        result.ShouldHaveValidationErrorFor(x => x.PreferredFit)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidFit.Code);
    }

    [Theory]
    [InlineData("slim")]
    [InlineData("regular")]
    [InlineData("relaxed-fit")]
    public void ApplyPreferredFitRules_WhenValid_ShouldPass(string fit)
    {
        var validator = new PreferredFitValidator();
        var result = validator.TestValidate(new PreferredFitTestModel { PreferredFit = fit });
        result.ShouldNotHaveValidationErrorFor(x => x.PreferredFit);
    }

    private sealed class FavoriteColorsTestModel
    {
        public List<string> FavoriteColors { get; set; } = [];
    }

    private sealed class FavoriteColorsValidator : AbstractValidator<FavoriteColorsTestModel>
    {
        public FavoriteColorsValidator()
        {
            RuleFor(x => x.FavoriteColors).ApplyFavoriteColorsRules();
        }
    }

    [Fact]
    public void ApplyFavoriteColorsRules_WhenTooMany_ShouldHaveError()
    {
        var validator = new FavoriteColorsValidator();
        var colors = Enumerable.Repeat("red", UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser + 1).ToList();
        var result = validator.TestValidate(new FavoriteColorsTestModel { FavoriteColors = colors });
        result.ShouldHaveValidationErrorFor(x => x.FavoriteColors)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyFavoriteColors.Code);
    }

    [Fact]
    public void ApplyFavoriteColorsRules_WhenWithinLimit_ShouldPass()
    {
        var validator = new FavoriteColorsValidator();
        var colors = Enumerable.Repeat("blue", UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser).ToList();
        var result = validator.TestValidate(new FavoriteColorsTestModel { FavoriteColors = colors });
        result.ShouldNotHaveValidationErrorFor(x => x.FavoriteColors);
    }

    private sealed class FavoriteCategoriesTestModel
    {
        public List<string> FavoriteCategories { get; set; } = [];
    }

    private sealed class FavoriteCategoriesValidator : AbstractValidator<FavoriteCategoriesTestModel>
    {
        public FavoriteCategoriesValidator()
        {
            RuleFor(x => x.FavoriteCategories).ApplyFavoriteCategoriesRules();
        }
    }

    [Fact]
    public void ApplyFavoriteCategoriesRules_WhenTooMany_ShouldHaveError()
    {
        var validator = new FavoriteCategoriesValidator();
        var categories = Enumerable.Repeat("shoes", UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser + 1)
            .ToList();
        var result = validator.TestValidate(new FavoriteCategoriesTestModel { FavoriteCategories = categories });
        result.ShouldHaveValidationErrorFor(x => x.FavoriteCategories)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyFavoriteCategories.Code);
    }

    [Fact]
    public void ApplyFavoriteCategoriesRules_WhenWithinLimit_ShouldPass()
    {
        var validator = new FavoriteCategoriesValidator();
        var categories = Enumerable.Repeat("hats", UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
            .ToList();
        var result = validator.TestValidate(new FavoriteCategoriesTestModel { FavoriteCategories = categories });
        result.ShouldNotHaveValidationErrorFor(x => x.FavoriteCategories);
    }

    private sealed class PreferredBrandsTestModel
    {
        public List<string> PreferredBrands { get; set; } = [];
    }

    private sealed class PreferredBrandsValidator : AbstractValidator<PreferredBrandsTestModel>
    {
        public PreferredBrandsValidator()
        {
            RuleFor(x => x.PreferredBrands).ApplyPreferredBrandsRules();
        }
    }

    [Fact]
    public void ApplyPreferredBrandsRules_WhenTooMany_ShouldHaveError()
    {
        var validator = new PreferredBrandsValidator();
        var brands = Enumerable.Repeat("nike", UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser + 1)
            .ToList();
        var result = validator.TestValidate(new PreferredBrandsTestModel { PreferredBrands = brands });
        result.ShouldHaveValidationErrorFor(x => x.PreferredBrands)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyPreferredBrands.Code);
    }

    [Fact]
    public void ApplyPreferredBrandsRules_WhenWithinLimit_ShouldPass()
    {
        var validator = new PreferredBrandsValidator();
        var brands = Enumerable.Repeat("adidas", UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser).ToList();
        var result = validator.TestValidate(new PreferredBrandsTestModel { PreferredBrands = brands });
        result.ShouldNotHaveValidationErrorFor(x => x.PreferredBrands);
    }

    private sealed class SizeTopTestModel
    {
        public string? SizeTop { get; set; }
    }

    private sealed class SizeTopValidator : AbstractValidator<SizeTopTestModel>
    {
        public SizeTopValidator()
        {
            RuleFor(x => x.SizeTop).ApplySizeTopRules();
        }
    }

    [Fact]
    public void ApplySizeTopRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new SizeTopValidator();
        var longSize = new string('X', UserPreferenceConstant.Constraints.MaxSizeTopLength + 1);
        var result = validator.TestValidate(new SizeTopTestModel { SizeTop = longSize });
        result.ShouldHaveValidationErrorFor(x => x.SizeTop);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("AB")]
    public void ApplySizeTopRules_WhenInvalidFormat_ShouldHaveError(string size)
    {
        var validator = new SizeTopValidator();
        var result = validator.TestValidate(new SizeTopTestModel { SizeTop = size });
        result.ShouldHaveValidationErrorFor(x => x.SizeTop)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeTop.Code);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("M")]
    [InlineData("L")]
    [InlineData("XL")]
    [InlineData("XXL")]
    [InlineData("30")]
    [InlineData("32")]
    public void ApplySizeTopRules_WhenValid_ShouldPass(string size)
    {
        var validator = new SizeTopValidator();
        var result = validator.TestValidate(new SizeTopTestModel { SizeTop = size });
        result.ShouldNotHaveValidationErrorFor(x => x.SizeTop);
    }

    private sealed class SizeBottomTestModel
    {
        public string? SizeBottom { get; set; }
    }

    private sealed class SizeBottomValidator : AbstractValidator<SizeBottomTestModel>
    {
        public SizeBottomValidator()
        {
            RuleFor(x => x.SizeBottom).ApplySizeBottomRules();
        }
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("AB")]
    public void ApplySizeBottomRules_WhenInvalidFormat_ShouldHaveError(string size)
    {
        var validator = new SizeBottomValidator();
        var result = validator.TestValidate(new SizeBottomTestModel { SizeBottom = size });
        result.ShouldHaveValidationErrorFor(x => x.SizeBottom)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeBottom.Code);
    }

    [Theory]
    [InlineData("30")]
    [InlineData("32")]
    [InlineData("34")]
    [InlineData("S")]
    [InlineData("M")]
    [InlineData("L")]
    [InlineData("XL")]
    public void ApplySizeBottomRules_WhenValid_ShouldPass(string size)
    {
        var validator = new SizeBottomValidator();
        var result = validator.TestValidate(new SizeBottomTestModel { SizeBottom = size });
        result.ShouldNotHaveValidationErrorFor(x => x.SizeBottom);
    }

    private sealed class ShoeSizeTestModel
    {
        public string? ShoeSize { get; set; }
    }

    private sealed class ShoeSizeValidator : AbstractValidator<ShoeSizeTestModel>
    {
        public ShoeSizeValidator()
        {
            RuleFor(x => x.ShoeSize).ApplyShoeSizeRules();
        }
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("AB")]
    public void ApplyShoeSizeRules_WhenInvalidFormat_ShouldHaveError(string size)
    {
        var validator = new ShoeSizeValidator();
        var result = validator.TestValidate(new ShoeSizeTestModel { ShoeSize = size });
        result.ShouldHaveValidationErrorFor(x => x.ShoeSize)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidShoeSize.Code);
    }

    [Theory]
    [InlineData("7")]
    [InlineData("8")]
    [InlineData("9.5")]
    [InlineData("10")]
    [InlineData("42")]
    public void ApplyShoeSizeRules_WhenValid_ShouldPass(string size)
    {
        var validator = new ShoeSizeValidator();
        var result = validator.TestValidate(new ShoeSizeTestModel { ShoeSize = size });
        result.ShouldNotHaveValidationErrorFor(x => x.ShoeSize);
    }

    private sealed class UserPreferencesTestModel
    {
        public UserPreferences Preferences { get; set; } = new();
    }

    private sealed class UserPreferencesValidatorWrapper : AbstractValidator<UserPreferencesTestModel>
    {
        public UserPreferencesValidatorWrapper()
        {
            RuleFor(x => x.Preferences).ValidateUserPreferences();
        }
    }

    [Fact]
    public void ValidateUserPreferences_WhenPreferencesWithExcessColors_ShouldHaveError()
    {
        var validator = new UserPreferencesValidatorWrapper();
        var preferences = new UserPreferences
        {
            PreferredStyle = "casual",
            PreferredFit = "regular",
            FavoriteColors = Enumerable
                .Repeat("red", UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser + 1).ToList()
        };
        var result = validator.TestValidate(new UserPreferencesTestModel { Preferences = preferences });
        result.ShouldHaveValidationErrorFor("Preferences.FavoriteColors");
    }
}