using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class GetShippingRates
{
    public sealed record Response : ShippingRateDetailResponse;
}
