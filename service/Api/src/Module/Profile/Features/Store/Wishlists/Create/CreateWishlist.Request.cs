namespace Module.Profile.Features.Store.Wishlists.Create;

public static partial class CreateWishlist
{
    public sealed class Request
    {
        public string Name { get; init; } = string.Empty;
        public bool IsPrivate { get; init; }
    }
}
