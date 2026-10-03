// Policy: Size patterns follow standard apparel notation (S/M/L/XL); color count limits prevent abuse
namespace Module.Customer.Domain.Preferences;

public static class UserPreferenceConstant
{
    public static class Defaults
    {
        public const string PreferredStyle = "casual";
        public const string PreferredFit = "regular";
    }

    public static class Constraints
    {
        public const int MaxPreferredStyleLength = 50;
        public const int MaxPreferredFitLength = 50;
        public const int MaxSizeTopLength = 10;
        public const int MaxSizeBottomLength = 10;
        public const int MaxShoeSizeLength = 10;
        public const int MaxFavoriteColorsPerUser = 10;
        public const int MaxFavoriteCategoriesPerUser = 10;
        public const int MaxPreferredBrandsPerUser = 10;
        public const int MaxFavoriteColorLength = 30;
        public const int MaxFavoriteCategoryLength = 50;
        public const int MaxPreferredBrandLength = 50;
    }

    public static class Patterns
    {
        // Allow letters, numbers, spaces, and common special characters for style/fit
        public const string StyleFitPattern = @"^[a-zA-Z0-9\s\-_]+$";
        // Allow letters, numbers, spaces for colors/categories/brands
        public const string TextPattern = @"^[a-zA-Z0-9\s]+$";
        // Standard size patterns (e.g., S, M, L, XL, XS, etc.)
        public const string SizePattern = @"^(?:XS|S|M|L|XL|XXL|XXXL|\d+(?:\.\d+)?(?:\s*(?:XS|S|M|L|XL|XXL|XXXL))?)$";
    }

    public static class AllowedStyles
    {
        public static readonly string[] Values = ["Casual", "Streetwear", "Formal", "Minimalist", "Sporty", "Bohemian", "Vintage", "Classic"];
    }

    public static class AllowedFits
    {
        public static readonly string[] Values = ["Slim", "Regular", "Relaxed", "Oversized", "Skinny", "Loose"];
    }
}