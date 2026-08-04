namespace Module.Profile.Features.Storefront.Wishlists.Shared.Models;

public abstract record WishlistParameters
{
    public string Name { get; init; } = string.Empty;
    public bool IsPrivate { get; init; }
}