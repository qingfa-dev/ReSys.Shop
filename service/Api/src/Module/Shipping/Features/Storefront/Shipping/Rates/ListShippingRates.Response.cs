using Module.Shipping.Features.Admin.ShippingRates.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    public sealed record Response : ShippingRateDetailResponse;
}
