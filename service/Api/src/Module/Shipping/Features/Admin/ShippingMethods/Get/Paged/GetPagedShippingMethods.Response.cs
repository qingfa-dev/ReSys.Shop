using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;

public static partial class GetPagedShippingMethods
{
    public record Response : ShippingMethodListItemResponse;
}