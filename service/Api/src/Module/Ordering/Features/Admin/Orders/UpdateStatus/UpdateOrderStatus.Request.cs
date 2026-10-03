using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

public static partial class UpdateOrderStatus
{
    public sealed record Request : OrderStatusUpdateParameters;
}