namespace Module.Customer.Features.Storefront.Shared.Models;

public abstract record WishlistParameters
{
    public string Name { get; init; } = string.Empty;
    public bool IsPrivate { get; init; }
}

public record WishlistRequest : WishlistParameters;

public record WishlistDetailResponse : WishlistParameters
{
    public Guid Id { get; init; }
    public string Token { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public bool IsDefault { get; init; }
    public List<WishedItemResponse> WishedItems { get; init; } = [];
}

public record WishlistListItemResponse : WishlistParameters
{
    public Guid Id { get; init; }
    public int ItemCount { get; init; }
}
