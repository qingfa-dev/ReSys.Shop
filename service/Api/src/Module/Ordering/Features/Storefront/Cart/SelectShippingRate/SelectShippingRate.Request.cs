using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    public sealed record Request : ShippingMethodActionParameters;
}
