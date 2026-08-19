using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    public sealed record Request : ShippingMethodActionParameters;
}
