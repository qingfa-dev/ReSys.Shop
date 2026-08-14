using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    /// <summary>Handler for getting the ordering dashboard data.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets the ordering dashboard data.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Base query filtering out soft-deleted orders
            var baseQuery = dbContext.Set<Order>().Where(o => !o.IsDeleted);

            // Aggregate: Compute total orders and revenue across all non-deleted orders
            var totalOrders = await baseQuery.CountAsync(cancellationToken);
            var totalRevenue = await baseQuery.SumAsync(o => o.Total, cancellationToken);
            var pendingFulfillment = await baseQuery.CountAsync(o => o.Status == OrderStatus.Placed, cancellationToken);

            // Compute: Count orders placed today for daily activity metrics
            var todayStart = DateTimeOffset.UtcNow.Date;
            var todayOrders = await baseQuery.CountAsync(o => o.CreatedAtUtc >= todayStart, cancellationToken);

            // Aggregate: Breakdown by status for the dashboard status pie chart
            var statusBreakdown = new OrderStatusBreakdownData
            {
                Draft = await baseQuery.CountAsync(o => o.Status == OrderStatus.Draft, cancellationToken),
                Placed = await baseQuery.CountAsync(o => o.Status == OrderStatus.Placed, cancellationToken),
                Canceled = await baseQuery.CountAsync(o => o.Status == OrderStatus.Canceled, cancellationToken),
                Expired = await baseQuery.CountAsync(o => o.Status == OrderStatus.Expired, cancellationToken),
            };

            // Load: Fetch the 10 most recent orders for the dashboard activity feed
            var recentOrders = await baseQuery
                .OrderByDescending(o => o.CreatedAtUtc)
                .Take(10)
                .Select(o => new RecentOrderData
                {
                    Id = o.Id,
                    Number = o.Number,
                    Total = o.Total,
                    Status = o.Status.ToString(),
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new Response
            {
                TotalOrders = totalOrders,
                PendingFulfillment = pendingFulfillment,
                TodayOrders = todayOrders,
                AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                StatusBreakdown = statusBreakdown
            };
        }
    }
}
