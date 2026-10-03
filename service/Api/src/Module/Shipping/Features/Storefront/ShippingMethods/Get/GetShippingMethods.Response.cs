using Module.Shipping.Features.Storefront.Shared.Models;

namespace Module.Shipping.Features.Storefront.ShippingMethods.Get;

public static partial class GetShippingMethods
{
    public sealed record Response : StorefrontShippingMethodListItem;
}
