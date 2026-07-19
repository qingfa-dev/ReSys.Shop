using Module.Ordering.Domain.Orders;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var baseQuery = dbContext.Set<Order>().Where(o => !o.IsDeleted);

            var totalOrders = await baseQuery.CountAsync(cancellationToken);
            var totalRevenue = await baseQuery.SumAsync(o => o.Total, cancellationToken);
            var pendingFulfillment = await baseQuery.CountAsync(o => o.Status == OrderStatus.Placed, cancellationToken);

            var todayStart = DateTimeOffset.UtcNow.Date;
            var todayOrders = await baseQuery.CountAsync(o => o.CreatedAtUtc >= todayStart, cancellationToken);

            var statusBreakdown = new OrderStatusBreakdownData
            {
                Draft = await baseQuery.CountAsync(o => o.Status == OrderStatus.Draft, cancellationToken),
                Placed = await baseQuery.CountAsync(o => o.Status == OrderStatus.Placed, cancellationToken),
                Canceled = await baseQuery.CountAsync(o => o.Status == OrderStatus.Canceled, cancellationToken),
                Expired = await baseQuery.CountAsync(o => o.Status == OrderStatus.Expired, cancellationToken),
            };

            var recentOrders = await baseQuery
                .OrderByDescending(o => o.CreatedAtUtc)
                .Take(10)
                .Select(o => new RecentOrderData
                {
                    Id = o.Id,
                    Number = o.Number,
                    Total = o.Total,
                    Status = o.Status.ToString(),
                    CreatedAtUtc = o.CreatedAtUtc.DateTime
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
