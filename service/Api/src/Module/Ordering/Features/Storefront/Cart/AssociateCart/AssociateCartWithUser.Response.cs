namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;
public static partial class AssociateCartWithUser
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public int ItemCount { get; init; }
    }
}
