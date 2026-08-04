namespace Module.Profile.Features.Storefront.Wishlists.AddItem;

public static partial class AddWishlistItem
{
    public sealed class Request
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; } = 1;
    }
}