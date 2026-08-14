using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

public static partial class UpdateOrderShipmentState
{
    public sealed record Request
    {
        public OrderFulfillmentState? FulfillmentState { get; init; }
    }
}
