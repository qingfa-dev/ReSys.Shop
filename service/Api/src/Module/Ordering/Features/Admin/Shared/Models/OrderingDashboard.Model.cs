namespace Module.Ordering.Features.Admin.Shared.Models;

public abstract record OrderingDashboardParameters
{
    public int TotalOrders { get; init; }
    public int PendingFulfillment { get; init; }
    public int TodayOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal TotalRevenue { get; init; }
    public List<RecentOrderData> RecentOrders { get; init; } = [];
    public OrderStatusBreakdownData StatusBreakdown { get; init; } = new();
}

public sealed record RecentOrderData
{
    public Guid Id { get; init; }
    public string Number { get; init; } = default!;
    public decimal Total { get; init; }
    public string Status { get; init; } = default!;
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record OrderStatusBreakdownData
{
    public int Draft { get; init; }
    public int Placed { get; init; }
    public int Canceled { get; init; }
    public int Expired { get; init; }
}
