namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    public sealed record Response
    {
        public int TotalOrders { get; init; }
        public int PendingFulfillment { get; init; }
        public int TodayOrders { get; init; }
        public decimal AverageOrderValue { get; init; }
        public decimal TotalRevenue { get; init; }
        public List<RecentOrderData> RecentOrders { get; init; } = [];
        public OrderStatusBreakdownData StatusBreakdown { get; init; } = new();
    }

    public sealed record RecentOrderData(Guid Id, string Number, decimal Total, string Status, DateTime CreatedAtUtc);

    public sealed record OrderStatusBreakdownData
    {
        public int Draft { get; init; }
        public int Placed { get; init; }
        public int Canceled { get; init; }
        public int Expired { get; init; }
    }
}
