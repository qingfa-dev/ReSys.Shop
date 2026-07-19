using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    /// <summary>Handler for getting the inventory dashboard data.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets the inventory dashboard data.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var locations = await dbContext.Set<StockLocation>()
                .Where(sl => sl.Active && !sl.IsDeleted)
                .ToListAsync(cancellationToken);

            var locationIds = locations.Select(l => l.Id).ToHashSet();
            var stockItems = await dbContext.Set<StockItem>()
                .Where(si => locationIds.Contains(si.StockLocationId))
                .ToListAsync(cancellationToken);

            var groupedByVariant = stockItems
                .GroupBy(si => si.VariantId)
                .ToList();

            var totalSkusTracked = groupedByVariant.Count;
            var outOfStockCount = 0;
            var lowStockCount = 0;

            foreach (var group in groupedByVariant)
            {
                var totalOnHand = group.Sum(si => si.CountOnHand);
                if (totalOnHand == 0)
                {
                    outOfStockCount++;
                    continue;
                }

                if (group.Any(si =>
                {
                    var loc = locations.FirstOrDefault(l => l.Id == si.StockLocationId);
                    return loc != null && si.CountOnHand <= loc.LowStockThreshold;
                }))
                {
                    lowStockCount++;
                }
            }

            var inStockCount = totalSkusTracked - outOfStockCount;
            var stockLocationCount = locations.Count;
            var itemsPerLocationAverage = stockLocationCount > 0
                ? (int)Math.Round((double)stockItems.Count / stockLocationCount)
                : 0;

            var recentMovements = await dbContext.Set<StockMovement>()
                .Where(sm => sm.StockLocationId == null || locationIds.Contains(sm.StockLocationId.Value))
                .OrderByDescending(sm => sm.CreatedAtUtc)
                .Take(10)
                .Select(sm => new RecentMovementData
                {
                    Id = sm.Id,
                    Quantity = sm.Quantity,
                    Action = sm.Action,
                    Reason = sm.Reason,
                    CreatedAtUtc = sm.CreatedAtUtc.DateTime
                })
                .ToListAsync(cancellationToken);

            return new Response
            {
                TotalSkusTracked = totalSkusTracked,
                InStockCount = inStockCount,
                OutOfStockCount = outOfStockCount,
                LowStockCount = lowStockCount,
                StockLocationCount = stockLocationCount,
                ItemsPerLocationAverage = itemsPerLocationAverage,
                RecentMovements = recentMovements
            };
        }
    }
}
