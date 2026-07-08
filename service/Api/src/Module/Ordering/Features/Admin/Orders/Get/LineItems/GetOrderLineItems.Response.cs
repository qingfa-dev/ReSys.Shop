namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;
public static partial class GetOrderLineItems
{
    public class Response
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal Total { get; init; }
        public decimal AdjustmentTotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
