namespace Module.Ordering.Features.Storefront.Orders.ListOrders;

public static partial class ListCustomerOrders
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = null!;
        /// <summary>Order status as a string (e.g. Draft, Placed, Canceled).</summary>
        public string Status { get; init; } = null!;
        public decimal Total { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
