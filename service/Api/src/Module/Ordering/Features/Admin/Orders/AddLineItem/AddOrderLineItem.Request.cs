namespace Module.Ordering.Features.Admin.Orders.AddLineItem;

public static partial class AddOrderLineItem
{
    public record Request
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; } = 1;
        /// <summary>Unit price at the time the line item is added — snapshotted to prevent drift from catalogue changes.</summary>
        public decimal Price { get; init; }
    }
}
