namespace Module.Ordering.Features.Storefront.Orders.ListOrders;

public static partial class ListCustomerOrders
{
    /// <summary>Storefront order list — intentionally standalone (not inheriting OrderListItemResponse).
    /// This is a lean projection (5 fields, string Status) vs admin OrderListItemResponse (13 fields, OrderStatus enum).
    /// Using a separate response type prevents leaking admin fields (PaymentState, ShipmentState, addresses) to storefront API
    /// and avoids changing Status from string to enum (which would break frontend serialization).</summary>
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = null!;
        /// <summary>Order status as a string (e.g. Draft, Placed, Canceled).</summary>
        public string Status { get; init; } = null!;
        public decimal Total { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
