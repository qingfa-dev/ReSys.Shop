namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    public sealed record Request
    {
        public string? PaymentIntentId { get; init; }
    }
}
