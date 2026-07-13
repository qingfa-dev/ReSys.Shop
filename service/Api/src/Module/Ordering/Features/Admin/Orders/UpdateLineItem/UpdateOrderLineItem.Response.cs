namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;

public static partial class UpdateOrderLineItem
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public int Quantity { get; init; }
        /// <summary>Recalculated line item total after quantity change.</summary>
        public decimal Total { get; init; }
    }
}
