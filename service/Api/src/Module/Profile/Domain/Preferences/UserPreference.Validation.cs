// Validate: FluentValidation rules for UserPreferences — pattern matching for sizes, count limits
namespace Module.Profile.Domain.Preferences;

public static class UserPreferenceValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyPreferredStyleRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserPreferenceConstant.Constraints.MaxPreferredStyleLength)
            .WithErrorCode(UserPreferencesResult.Failure.StyleTooLong.Code)
            .WithMessage(UserPreferencesResult.Failure.StyleTooLong.Message)
            .Matches(UserPreferenceConstant.Patterns.StyleFitPattern)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidStyle.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidStyle.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPreferredFitRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserPreferenceConstant.Constraints.MaxPreferredFitLength)
            .WithErrorCode(UserPreferencesResult.Failure.FitTooLong.Code)
            .WithMessage(UserPreferencesResult.Failure.FitTooLong.Message)
            .Matches(UserPreferenceConstant.Patterns.StyleFitPattern)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidFit.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidFit.Message);
    }

    public static IRuleBuilderOptions<T, List<string>> ApplyFavoriteColorsRules<T>(this IRuleBuilder<T, List<string>> ruleBuilder)
    {
        return ruleBuilder
            .Must(colors => colors == null || colors.Count <= UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyFavoriteColors.Code)
            .WithMessage(UserPreferencesResult.Failure.TooManyFavoriteColors.Message)
            .Must(colors => colors == null || colors.All(c => c.Length <= UserPreferenceConstant.Constraints.MaxFavoriteColorLength))
            .WithErrorCode(UserPreferencesResult.Failure.FavoriteColorItemTooLong.Code)
            .WithMessage(UserPreferencesResult.Failure.FavoriteColorItemTooLong.Message);
    }

    public static IRuleBuilderOptions<T, List<string>> ApplyFavoriteCategoriesRules<T>(this IRuleBuilder<T, List<string>> ruleBuilder)
    {
        return ruleBuilder
            .Must(categories => categories == null || categories.Count <= UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyFavoriteCategories.Code)
            .WithMessage(UserPreferencesResult.Failure.TooManyFavoriteCategories.Message)
            .Must(categories => categories == null || categories.All(c => c.Length <= UserPreferenceConstant.Constraints.MaxFavoriteCategoryLength))
            .WithErrorCode(UserPreferencesResult.Failure.FavoriteCategoryItemTooLong.Code)
            .WithMessage(UserPreferencesResult.Failure.FavoriteCategoryItemTooLong.Message);
    }

    public static IRuleBuilderOptions<T, List<string>> ApplyPreferredBrandsRules<T>(this IRuleBuilder<T, List<string>> ruleBuilder)
    {
        return ruleBuilder
            .Must(brands => brands == null || brands.Count <= UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser)
            .WithErrorCode(UserPreferencesResult.Failure.TooManyPreferredBrands.Code)
            .WithMessage(UserPreferencesResult.Failure.TooManyPreferredBrands.Message)
            .Must(brands => brands == null || brands.All(b => b.Length <= UserPreferenceConstant.Constraints.MaxPreferredBrandLength))
            .WithErrorCode(UserPreferencesResult.Failure.PreferredBrandItemTooLong.Code)
            .WithMessage(UserPreferencesResult.Failure.PreferredBrandItemTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySizeTopRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserPreferenceConstant.Constraints.MaxSizeTopLength)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeTop.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidSizeTop.Message)
            .Matches(UserPreferenceConstant.Patterns.SizePattern)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeTop.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidSizeTop.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySizeBottomRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserPreferenceConstant.Constraints.MaxSizeBottomLength)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeBottom.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidSizeBottom.Message)
            .Matches(UserPreferenceConstant.Patterns.SizePattern)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidSizeBottom.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidSizeBottom.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyShoeSizeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserPreferenceConstant.Constraints.MaxShoeSizeLength)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidShoeSize.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidShoeSize.Message)
            .Matches(UserPreferenceConstant.Patterns.SizePattern)
            .WithErrorCode(UserPreferencesResult.Failure.InvalidShoeSize.Code)
            .WithMessage(UserPreferencesResult.Failure.InvalidShoeSize.Message);
    }

    public static IRuleBuilderOptions<T, UserPreferences> ValidateUserPreferences<T>(
        this IRuleBuilder<T, UserPreferences> ruleBuilder)
    {
        return ruleBuilder
            .SetValidator(new UserPreferenceValidator());
    }
}

public class UserPreferenceValidator : AbstractValidator<UserPreferences>
{
    public UserPreferenceValidator()
    {
        RuleFor(x => x.PreferredStyle).ApplyPreferredStyleRules();
        RuleFor(x => x.PreferredFit).ApplyPreferredFitRules();
        RuleFor(x => x.FavoriteColors).ApplyFavoriteColorsRules();
        RuleFor(x => x.FavoriteCategories).ApplyFavoriteCategoriesRules();
        RuleFor(x => x.PreferredBrands).ApplyPreferredBrandsRules();
        RuleFor(x => x.SizeTop).ApplySizeTopRules();
        RuleFor(x => x.SizeBottom).ApplySizeBottomRules();
        RuleFor(x => x.ShoeSize).ApplyShoeSizeRules();
    }
}