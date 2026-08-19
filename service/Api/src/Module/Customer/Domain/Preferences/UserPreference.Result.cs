namespace Module.Customer.Domain.Preferences;

public static class UserPreferencesResult
{
    public static class Success
    {
        public const string PreferencesCreated = "User preferences created successfully";
        public const string PreferencesUpdated = "User preferences updated successfully";
    }

    public static class Failure
    {
        public static Error InvalidStyle => Error.Validation(
            code: "UserPreferences.Style.Invalid",
            message: "Invalid preferred style. Allowed values: casual, formal, sporty, classic, bohemian, minimalist, streetwear");

        public static Error InvalidFit => Error.Validation(
            code: "UserPreferences.Fit.Invalid",
            message: "Invalid preferred fit. Allowed values: slim, regular, relaxed, oversized, athletic");

        public static Error TooManyFavoriteColors => Error.Validation(
            code: "UserPreferences.FavoriteColors.LimitExceeded",
            message: $"Cannot have more than {UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser} favorite colors");

        public static Error TooManyFavoriteCategories => Error.Validation(
            code: "UserPreferences.FavoriteCategories.LimitExceeded",
            message: $"Cannot have more than {UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser} favorite categories");

        public static Error TooManyPreferredBrands => Error.Validation(
            code: "UserPreferences.PreferredBrands.LimitExceeded",
            message: $"Cannot have more than {UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser} preferred brands");

        public static Error InvalidSizeTop => Error.Validation(
            code: "UserPreferences.SizeTop.Invalid",
            message: "Invalid top size. Allowed values: XS, S, M, L, XL, XXL, XXXL");

        public static Error InvalidSizeBottom => Error.Validation(
            code: "UserPreferences.SizeBottom.Invalid",
            message: "Invalid bottom size. Allowed values: 28-44");

        public static Error InvalidShoeSize => Error.Validation(
            code: "UserPreferences.ShoeSize.Invalid",
            message: "Invalid shoe size");

        public static Error StyleTooLong => Error.Validation(
            code: "UserPreferences.Style.TooLong",
            message: $"Preferred style cannot exceed {UserPreferenceConstant.Constraints.MaxPreferredStyleLength} characters.");

        public static Error FitTooLong => Error.Validation(
            code: "UserPreferences.Fit.TooLong",
            message: $"Preferred fit cannot exceed {UserPreferenceConstant.Constraints.MaxPreferredFitLength} characters.");

        public static Error FavoriteColorItemTooLong => Error.Validation(
            code: "UserPreferences.FavoriteColor.ItemTooLong",
            message: $"Each favorite color cannot exceed {UserPreferenceConstant.Constraints.MaxFavoriteColorLength} characters.");

        public static Error FavoriteCategoryItemTooLong => Error.Validation(
            code: "UserPreferences.FavoriteCategory.ItemTooLong",
            message: $"Each favorite category cannot exceed {UserPreferenceConstant.Constraints.MaxFavoriteCategoryLength} characters.");

        public static Error PreferredBrandItemTooLong => Error.Validation(
            code: "UserPreferences.PreferredBrand.ItemTooLong",
            message: $"Each preferred brand cannot exceed {UserPreferenceConstant.Constraints.MaxPreferredBrandLength} characters.");
    }
}