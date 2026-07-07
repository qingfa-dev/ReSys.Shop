namespace Module.Profile.Features.Store.Wishlists.AddItem;

public static partial class AddWishlistItem
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsPrivate { get; init; }
        public bool IsDefault { get; init; }
        public string Token { get; init; } = string.Empty;
        public int ItemCount { get; init; }
    }
}
