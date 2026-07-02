// Contract: All error factories return Error instances with unique codes for traceability
namespace Module.Profile.Domain.Wishlists;

/// <summary>Contains success messages and error factory methods for Wishlist operations.</summary>
public static class WishlistResult
{
    /// <summary>Success message factory for Wishlist operations.</summary>
    public static class Success
    {
        public const string Created = "Wishlist created successfully.";
        public const string Updated = "Wishlist updated successfully.";
        public const string Retrieved = "Wishlist retrieved successfully.";
        public const string Shared = "Wishlist shared successfully.";
        public const string Cleared = "Wishlist cleared successfully.";
        public const string ItemAdded = "Item added to wishlist successfully.";
        public const string ItemRemoved = "Item removed from wishlist successfully.";
        public const string Merged = "Wishlists merged successfully.";
    }

    /// <summary>Error factory methods returning typed Error instances for Wishlist operations.</summary>
    public static class Failure
    {
        /// <summary>Wishlist not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Wishlist.NotFound",
            message: "Wishlist not found.");

        public static Error NameRequired => Error.Validation(
            code: "Wishlist.Name.Required",
            message: "Wishlist name is required.");

        public static Error NameTooLong => Error.Validation(
            code: "Wishlist.Name.TooLong",
            message: $"Wishlist name cannot exceed {WishlistConstant.Constraints.MaxNameLength} characters.");

        public static Error UserIdRequired => Error.Validation(
            code: "Wishlist.UserId.Required",
            message: "User ID is required.");

        public static Error TokenRequired => Error.Validation(
            code: "Wishlist.Token.Required",
            message: "Token is required.");

        public static Error TokenTooLong => Error.Validation(
            code: "Wishlist.Token.TooLong",
            message: $"Token cannot exceed {WishlistConstant.Constraints.MaxTokenLength} characters.");

        public static Error ItemNotFound => Error.NotFound(
            code: "Wishlist.Item.NotFound",
            message: "Item not found in wishlist.");

        public static Error ItemAlreadyExists => Error.Conflict(
            code: "Wishlist.Item.AlreadyExists",
            message: "Item already exists in wishlist.");

        public static Error MaxItemsReached => Error.Validation(
            code: "Wishlist.MaxItems.Reached",
            message: $"Maximum number of items ({WishlistConstant.Constraints.MaxWishedItemsCount}) reached.");

        public static Error AlreadyShared => Error.Conflict(
            code: "Wishlist.AlreadyShared",
            message: "Wishlist is already shared.");

        public static Error AlreadyPrivate => Error.Conflict(
            code: "Wishlist.AlreadyPrivate",
            message: "Wishlist is already private.");

        /// <summary>Cannot delete the default wishlist.</summary>
        public static Error CannotDeleteDefault => Error.Validation(
            code: "Wishlist.Default.CannotDelete",
            message: "Cannot delete the default wishlist.");

        /// <summary>Authentication required for wishlist operations.</summary>
        public static Error AuthRequired => Error.Forbidden(
            code: "Wishlist.Auth.Required",
            message: "Authentication required.");
    }
}
