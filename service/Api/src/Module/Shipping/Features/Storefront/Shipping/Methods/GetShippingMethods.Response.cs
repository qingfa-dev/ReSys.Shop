using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public sealed record Response : ShippingMethodDetailResponse;
}
