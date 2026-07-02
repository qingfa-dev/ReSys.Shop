using Microsoft.CodeAnalysis;

namespace Module.Profile.Domain.Preferences;

public static class UserPreferenceMethod
{
    #region Factory Methods

    public static Result<UserPreferences> Create(
        string? preferredStyle = null,
        string? preferredFit = null,
        List<string>? favoriteColors = null,
        List<string>? favoriteCategories = null,
        List<string>? preferredBrands = null,
        string? sizeTop = null,
        string? sizeBottom = null,
        string? shoeSize = null)
    {
        if (preferredStyle?.Length > UserPreferenceConstant.Constraints.MaxPreferredStyleLength)
            return UserPreferencesResult.Failure.StyleTooLong;
        if (preferredFit?.Length > UserPreferenceConstant.Constraints.MaxPreferredFitLength)
            return UserPreferencesResult.Failure.FitTooLong;
        if (favoriteColors?.Count > UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser)
            return UserPreferencesResult.Failure.TooManyFavoriteColors;
        if (favoriteCategories?.Count > UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
            return UserPreferencesResult.Failure.TooManyFavoriteCategories;
        if (preferredBrands?.Count > UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser)
            return UserPreferencesResult.Failure.TooManyPreferredBrands;
        if (sizeTop?.Length > UserPreferenceConstant.Constraints.MaxSizeTopLength)
            return UserPreferencesResult.Failure.InvalidSizeTop;
        if (sizeBottom?.Length > UserPreferenceConstant.Constraints.MaxSizeBottomLength)
            return UserPreferencesResult.Failure.InvalidSizeBottom;
        if (shoeSize?.Length > UserPreferenceConstant.Constraints.MaxShoeSizeLength)
            return UserPreferencesResult.Failure.InvalidShoeSize;

        return new UserPreferences
        {
            PreferredStyle = preferredStyle ?? UserPreferenceConstant.Defaults.PreferredStyle,
            PreferredFit = preferredFit ?? UserPreferenceConstant.Defaults.PreferredFit,
            FavoriteColors = favoriteColors ?? [],
            FavoriteCategories = favoriteCategories ?? [],
            PreferredBrands = preferredBrands ?? [],
            SizeTop = sizeTop,
            SizeBottom = sizeBottom,
            ShoeSize = shoeSize
        };
    }

    #endregion

    #region Update

    public static Result<UserPreferences> Update(
        this UserPreferences prefs,
        Optional<string?> preferredStyle = default,
        Optional<string?> preferredFit = default,
        Optional<List<string>?> favoriteColors = default,
        Optional<List<string>?> favoriteCategories = default,
        Optional<List<string>?> preferredBrands = default,
        Optional<string?> sizeTop = default,
        Optional<string?> sizeBottom = default,
        Optional<string?> shoeSize = default)
    {
        if (preferredStyle.HasValue)
        {
            if (preferredStyle.Value?.Length > UserPreferenceConstant.Constraints.MaxPreferredStyleLength)
                return UserPreferencesResult.Failure.StyleTooLong;
            prefs.PreferredStyle = preferredStyle.Value ?? UserPreferenceConstant.Defaults.PreferredStyle;
        }

        if (preferredFit.HasValue)
        {
            if (preferredFit.Value?.Length > UserPreferenceConstant.Constraints.MaxPreferredFitLength)
                return UserPreferencesResult.Failure.FitTooLong;
            prefs.PreferredFit = preferredFit.Value ?? UserPreferenceConstant.Defaults.PreferredFit;
        }

        if (favoriteColors.HasValue)
        {
            List<string>? colors = favoriteColors.Value;
            if (colors is not null && colors.Count > UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser)
                return UserPreferencesResult.Failure.TooManyFavoriteColors;
            prefs.FavoriteColors = colors ?? [];
        }

        if (favoriteCategories.HasValue)
        {
            List<string>? categories = favoriteCategories.Value;
            if (categories is not null &&
                categories.Count > UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
                return UserPreferencesResult.Failure.TooManyFavoriteCategories;
            prefs.FavoriteCategories = categories ?? [];
        }

        if (preferredBrands.HasValue)
        {
            List<string>? brands = preferredBrands.Value;
            if (brands is not null && brands.Count > UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser)
                return UserPreferencesResult.Failure.TooManyPreferredBrands;
            prefs.PreferredBrands = brands ?? [];
        }

        if (sizeTop.HasValue)
        {
            if (sizeTop.Value?.Length > UserPreferenceConstant.Constraints.MaxSizeTopLength)
                return UserPreferencesResult.Failure.InvalidSizeTop;
            prefs.SizeTop = sizeTop.Value;
        }

        if (sizeBottom.HasValue)
        {
            if (sizeBottom.Value?.Length > UserPreferenceConstant.Constraints.MaxSizeBottomLength)
                return UserPreferencesResult.Failure.InvalidSizeBottom;
            prefs.SizeBottom = sizeBottom.Value;
        }

        if (shoeSize.HasValue)
        {
            if (shoeSize.Value?.Length > UserPreferenceConstant.Constraints.MaxShoeSizeLength)
                return UserPreferencesResult.Failure.InvalidShoeSize;
            prefs.ShoeSize = shoeSize.Value;
        }

        return prefs;
    }

    #endregion
}