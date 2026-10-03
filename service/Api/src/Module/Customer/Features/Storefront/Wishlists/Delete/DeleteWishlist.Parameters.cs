namespace Module.Customer.Features.Storefront.Wishlists.Delete;

public static partial class DeleteWishlist
{
    public sealed record Parameters
    {
        public Guid UserId { get; init; }
        public Guid Id { get; init; }
        public string? DeletedBy { get; init; }
    }
}
