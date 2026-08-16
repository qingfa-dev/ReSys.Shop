using Module.Ordering.Features.Storefront.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public sealed record Request : CartParameters
    {
    }
}