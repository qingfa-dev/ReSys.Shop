namespace Module.Profile.Features.Store.Wishlists.Get;

public static partial class GetWishlists
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsPrivate { get; init; }
        public bool IsDefault { get; init; }
        public int ItemCount { get; init; }
    }
}
