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
        string? preferredStyle = default,
        string? preferredFit = default,
        List<string>? favoriteColors = default,
        List<string>? favoriteCategories = default,
        List<string>? preferredBrands = default,
        string? sizeTop = default,
        string? sizeBottom = default,
        string? shoeSize = default)
    {
        if (preferredStyle is not null)
        {
            if (preferredStyle.Length > UserPreferenceConstant.Constraints.MaxPreferredStyleLength)
                return UserPreferencesResult.Failure.StyleTooLong;
            prefs.PreferredStyle = preferredStyle;
        }

        if (preferredFit is not null)
        {
            if (preferredFit.Length > UserPreferenceConstant.Constraints.MaxPreferredFitLength)
                return UserPreferencesResult.Failure.FitTooLong;
            prefs.PreferredFit = preferredFit;
        }

        if (favoriteColors is not null)
        {
            if (favoriteColors.Count > UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser)
                return UserPreferencesResult.Failure.TooManyFavoriteColors;
            prefs.FavoriteColors = favoriteColors;
        }

        if (favoriteCategories is not null)
        {
            if (favoriteCategories.Count > UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
                return UserPreferencesResult.Failure.TooManyFavoriteCategories;
            prefs.FavoriteCategories = favoriteCategories;
        }

        if (preferredBrands is not null)
        {
            if (preferredBrands.Count > UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser)
                return UserPreferencesResult.Failure.TooManyPreferredBrands;
            prefs.PreferredBrands = preferredBrands;
        }

        if (sizeTop is not null)
        {
            if (sizeTop.Length > UserPreferenceConstant.Constraints.MaxSizeTopLength)
                return UserPreferencesResult.Failure.InvalidSizeTop;
            prefs.SizeTop = sizeTop;
        }

        if (sizeBottom is not null)
        {
            if (sizeBottom.Length > UserPreferenceConstant.Constraints.MaxSizeBottomLength)
                return UserPreferencesResult.Failure.InvalidSizeBottom;
            prefs.SizeBottom = sizeBottom;
        }

        if (shoeSize is not null)
        {
            if (shoeSize.Length > UserPreferenceConstant.Constraints.MaxShoeSizeLength)
                return UserPreferencesResult.Failure.InvalidShoeSize;
            prefs.ShoeSize = shoeSize;
        }

        return prefs;
    }

    #endregion
}