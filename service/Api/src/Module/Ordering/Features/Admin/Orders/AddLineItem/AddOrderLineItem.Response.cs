namespace Module.Ordering.Features.Admin.Orders.AddLineItem;

public static partial class AddOrderLineItem
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
        /// <summary>Line item total — quantity × unit price, before adjustments.</summary>
        public decimal Total { get; init; }
    }
}
