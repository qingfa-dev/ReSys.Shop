using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

public static partial class GetPagedOrders
{
    public sealed record Response : OrderListItemResponse;
}
