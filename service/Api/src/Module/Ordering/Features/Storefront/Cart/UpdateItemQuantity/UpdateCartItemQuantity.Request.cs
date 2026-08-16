using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    public sealed record Request : LineItemQuantityParameters;
}