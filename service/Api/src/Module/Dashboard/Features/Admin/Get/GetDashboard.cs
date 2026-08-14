using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Ordering.Domain.Orders;

namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    /// <summary>Handler for getting the main dashboard data.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets the main dashboard data.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var sevenDaysAgo = now.AddDays(-7);

            var response = new Response
            {
                Sales = await BuildSales(now, thirtyDaysAgo, sevenDaysAgo, cancellationToken),
                Catalog = await BuildCatalog(cancellationToken),
                Inventory = await BuildInventory(cancellationToken),
                RecentActivities = await BuildActivities(cancellationToken)
            };

            return response;
        }

        // Compute: Aggregate sales metrics — total revenue, order count, trend percentage, daily history
        private async Task<SalesSummaryData> BuildSales(
            DateTimeOffset now, DateTimeOffset thirtyDaysAgo, DateTimeOffset sevenDaysAgo,
            CancellationToken ct)
        {
            // Filter: Exclude draft and canceled orders from sales computation
            var baseQuery = dbContext.Set<Order>()
                .Where(o => !o.IsDeleted
                    && o.Status != OrderStatus.Draft
                    && o.Status != OrderStatus.Canceled);

            // Aggregate: Total revenue and order count across all non-draft, non-canceled orders
            var totalRevenue = await baseQuery.SumAsync(o => o.Total, ct);
            var orderCount = await baseQuery.CountAsync(ct);

            // Filter: Restrict to orders created within the last 30 days for trend analysis
            var recentQuery = baseQuery.Where(o => o.CreatedAtUtc >= thirtyDaysAgo);
            var recentRevenue = await recentQuery.SumAsync(o => o.Total, ct);
            var last7Revenue = await recentQuery
                .Where(o => o.CreatedAtUtc >= sevenDaysAgo)
                .SumAsync(o => o.Total, ct);

            var last7Avg = last7Revenue / 7m;
            var previous23Avg = (recentRevenue - last7Revenue) / 23m;
            var revenueTrendPercentage = previous23Avg > 0m
                ? Math.Round((last7Avg / previous23Avg - 1m) * 100m, 2)
                : 0m;

            var thirtyDaysStart = thirtyDaysAgo.Date;
            var dailyRevenue = await recentQuery
                .GroupBy(o => o.CreatedAtUtc.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.Total) })
                .ToListAsync(ct);

            var trendHistory = Enumerable.Range(0, 30)
                .Select(i => thirtyDaysStart.AddDays(i))
                .Select(date =>
                {
                    var entry = dailyRevenue.FirstOrDefault(d => d.Date == date);
                    return new TrendPoint
                    {
                        Date = DateOnly.FromDateTime(date),
                        Revenue = entry?.Revenue ?? 0m
                    };
                })
                .ToList();

            return new SalesSummaryData
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0m,
                RevenueTrendPercentage = revenueTrendPercentage,
                TrendHistory = trendHistory
            };
        }

        // Compute: Aggregate catalog metrics — products, variants, taxonomies, recent additions
        private async Task<CatalogSummaryData> BuildCatalog(CancellationToken ct)
        {
            // Filter: Exclude soft-deleted entities from catalog counts
            var productsQuery = dbContext.Set<Product>().Where(p => !p.IsDeleted);
            var variantsQuery = dbContext.Set<Variant>().Where(v => !v.IsDeleted);
            var taxonomiesQuery = dbContext.Set<Taxonomy>().Where(t => !t.IsDeleted);
            var taxonsQuery = dbContext.Set<Taxon>().Where(t => !t.IsDeleted);

            var totalProducts = await productsQuery.CountAsync(ct);
            var activeProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Active, ct);
            var totalVariants = await variantsQuery.CountAsync(ct);
            var totalTaxonomies = await taxonomiesQuery.CountAsync(ct);
            var totalTaxons = await taxonsQuery.CountAsync(ct);

            var recentProducts = await productsQuery
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(5)
                .Select(p => new RecentProductData { Id = p.Id, Name = p.Name, Slug = p.Slug, CreatedAtUtc = p.CreatedAtUtc })
                .ToListAsync(ct);

            return new CatalogSummaryData
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                TotalVariants = totalVariants,
                TotalTaxonomies = totalTaxonomies,
                TotalTaxons = totalTaxons,
                RecentlyAdded = recentProducts
            };
        }

        // Compute: Aggregate inventory metrics — variant count, out-of-stock, low-stock
        private async Task<InventorySummaryData> BuildInventory(CancellationToken ct)
        {
            // Load: Active stock locations for threshold comparisons
            var locations = await dbContext.Set<StockLocation>()
                .Where(sl => sl.Active && !sl.IsDeleted)
                .ToListAsync(ct);

            var stockItems = await dbContext.Set<StockItem>()
                .Where(si => locations.Select(l => l.Id).Contains(si.StockLocationId))
                .ToListAsync(ct);

            var groupedByVariant = stockItems
                .GroupBy(si => si.VariantId)
                .ToList();

            var totalVariants = groupedByVariant.Count;

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

            return new InventorySummaryData
            {
                TotalVariants = totalVariants,
                OutOfStockCount = outOfStockCount,
                LowStockCount = lowStockCount,
                StockAccuracyPercentage = 100.0m
            };
        }

        // Load: Aggregate recent order and stock movement activity for the dashboard feed
        private async Task<List<ActivityItemData>> BuildActivities(CancellationToken ct)
        {
            // Load: Fetch 20 most recent non-draft, non-canceled orders for activity feed
            var recentOrders = await dbContext.Set<Order>()
                .Where(o => !o.IsDeleted
                    && o.Status != OrderStatus.Draft
                    && o.Status != OrderStatus.Canceled)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Take(20)
                .Select(o => new ActivityItemData
                {
                    Id = o.Id,
                    Type = "Order",
                    Title = "Order #" + o.Number,
                    Description = o.ItemCount + " item(s) · " + o.Currency + " " + o.Total.ToString("F2"),
                    Status = o.Status.ToString(),
                    Timestamp = o.CreatedAtUtc
                })
                .ToListAsync(ct);

            // Load: Fetch 20 most recent stock movements for activity feed
            var recentMovements = await dbContext.Set<StockMovement>()
                .OrderByDescending(sm => sm.CreatedAtUtc)
                .Take(20)
                .Select(sm => new ActivityItemData
                {
                    Id = sm.Id,
                    Type = "Stock",
                    Title = "Stock: " + (sm.Action ?? "Movement"),
                    Description = sm.Quantity + " units",
                    Status = "Completed",
                    Timestamp = sm.CreatedAtUtc
                })
                .ToListAsync(ct);

            return recentOrders
                .Concat(recentMovements)
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .ToList();
        }
    }
}
