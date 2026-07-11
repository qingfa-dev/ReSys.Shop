namespace Module.Profile.Features.Store.Wishlists.Shared.Models;

public abstract record WishlistParameters
{
    public string Name { get; init; } = string.Empty;
    public bool IsPrivate { get; init; }
}
