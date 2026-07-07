namespace Module.Profile.Features.Store.Wishlists.Delete;

public static partial class DeleteWishlist
{
    public sealed record Response(Guid Id, string Name);
}
