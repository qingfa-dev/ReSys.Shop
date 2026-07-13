using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public record Request : CartParameters
    {
    }
}
