namespace Module.Profile.Domain.Wishlists.WishedItems;
/// <summary>Contains error factory methods for WishedItem operations.</summary>
public static class WishedItemResult
{
    /// <summary>Error factory methods returning typed Error instances for WishedItem operations.</summary>
    public static class Failure
    {
        public static Error VariantIdRequired => Error.Validation(
            code: "WishedItem.VariantId.Required",
            message: "Variant ID is required.");

        public static Error WishlistIdRequired => Error.Validation(
            code: "WishedItem.WishlistId.Required",
            message: "Wishlist ID is required.");

        public static Error QuantityTooLow => Error.Validation(
            code: "WishedItem.Quantity.TooLow",
            message: $"Quantity must be at least {WishedItemConstant.Constraints.MinQuantity}.");

        public static Error QuantityTooHigh => Error.Validation(
            code: "WishedItem.Quantity.TooHigh",
            message: $"Quantity cannot exceed {WishedItemConstant.Constraints.MaxQuantity}.");

        /// <summary>Authentication required for wishlist item operations.</summary>
        public static Error AuthRequired => Error.Forbidden(
            code: "WishedItem.Auth.Required",
            message: "Authentication required.");
    }
}
