namespace Module.Profile.Features.Store.Wishlists.RemoveItem;

public static partial class RemoveWishlistItem
{
    public sealed record Response(Guid Id, string Name);
}
