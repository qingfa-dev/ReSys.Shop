namespace Module.Ordering.Features.Admin.Orders.AddLineItem;

public static partial class AddOrderLineItem
{
    public class Response
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
        public decimal Total { get; init; }
    }
}
