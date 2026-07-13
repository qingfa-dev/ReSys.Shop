namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;

public static partial class UpdateOrderLineItem
{
    public sealed record Request
    {
        public int Quantity { get; init; }
    }
}