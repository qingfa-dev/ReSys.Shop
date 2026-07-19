namespace Module.Profile.Features.Store.Wishlists.Shared.Models;

public record WishlistDetailResponse : WishlistParameters, IResponse
{
    public Guid Id { get; init; }
    public string Token { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public bool IsDefault { get; init; }
    public List<WishedItemResponse> WishedItems { get; init; } = [];
}

public record WishlistListItemResponse : WishlistParameters, IResponse
{
    public Guid Id { get; init; }
    public int ItemCount { get; init; }
}