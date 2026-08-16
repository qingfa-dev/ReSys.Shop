using Module.Ordering.Features.Storefront.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.LineItems.AddLineItem;

public static partial class AddOrderLineItem
{
    public sealed record Request : CartRequest
    {
        public decimal Price { get; init; }
    }
}
