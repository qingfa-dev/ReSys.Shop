namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;

public static partial class UpdateOrderLineItem
{
    public class Response
    {
        public Guid Id { get; init; }
        public int Quantity { get; init; }
        public decimal Total { get; init; }
    }
}
