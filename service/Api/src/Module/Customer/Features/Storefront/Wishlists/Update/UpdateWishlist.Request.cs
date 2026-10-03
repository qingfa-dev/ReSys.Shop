namespace Module.Customer.Features.Storefront.Wishlists.Update;

public static partial class UpdateWishlist
{
    public sealed class Request
    {
        public string? Name { get; init; }
        public bool? IsPrivate { get; init; }
        public bool? IsDefault { get; init; }
    }
}