using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

public static partial class UpdateOrderStatus
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public OrderStatus Status { get; init; }
        public DateTimeOffset? UpdatedAt { get; init; }
    }
}
