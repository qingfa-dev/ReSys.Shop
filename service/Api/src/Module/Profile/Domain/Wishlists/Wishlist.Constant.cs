// Policy: Wishlist token length must be sufficient for uniqueness; max items prevents abuse
namespace Module.Profile.Domain.Wishlists;

public static class WishlistConstant
{
    public static class Defaults
    {
        public const bool IsDefault = false;
        public const bool IsPrivate = false;
        public const int TokenLength = 64;
    }

    public static class Constraints
    {
        public const int MaxNameLength = 200;
        public const int MaxTokenLength = 128;
        public const int MaxWishedItemsCount = 100;
    }
}
