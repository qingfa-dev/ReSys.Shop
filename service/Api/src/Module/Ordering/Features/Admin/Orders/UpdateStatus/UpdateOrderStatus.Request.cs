using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

public static partial class UpdateOrderStatus
{
    public class Request
    {
        public OrderStatus Status { get; init; }
    }
}
