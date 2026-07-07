namespace Module.Profile.Features.Store.Wishlists.GetById;

public static partial class GetWishlistById
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsPrivate { get; init; }
        public bool IsDefault { get; init; }
        public string Token { get; init; } = string.Empty;
        public int ItemCount { get; init; }
        public List<WishedItemResponse> WishedItems { get; init; } = [];
    }

    public sealed class WishedItemResponse
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
        public DateTimeOffset AddedAtUtc { get; init; }
    }
}
