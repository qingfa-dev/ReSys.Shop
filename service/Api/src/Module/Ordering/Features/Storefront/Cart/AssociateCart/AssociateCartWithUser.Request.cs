namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    public sealed record Request
    {
        public Guid GuestOrderId { get; init; }
    }
}