using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.Paged;

public static partial class GetPagedShippingRates
{
    public record Response : ShippingRateListItemResponse;
}