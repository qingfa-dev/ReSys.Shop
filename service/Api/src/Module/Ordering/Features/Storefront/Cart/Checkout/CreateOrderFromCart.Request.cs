using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    public sealed record Request : OrderCreationParameters;
}