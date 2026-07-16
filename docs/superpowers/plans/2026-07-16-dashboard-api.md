# Admin Dashboard API — GET /api/dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `GET /api/dashboard` endpoint that returns aggregated sales, inventory, catalog, and activity data to power the Admin SPA dashboard.

**Architecture:** Single vertical-slice feature in `Module/Dashboard/` — a read-only cross-cutting handler that queries Catalog, Ordering, and Inventory entities via `IApplicationDbContext`. No `Domain/` folder, no persistence, no separate module. Returns `Result<Response>` with 4 nested record types matching existing frontend `report.types.ts`.

**Tech Stack:** C# 13, .NET 10, EF Core (Npgsql), MediatR, Carter, FluentAssertions, xUnit, InMemory EF Core (tests)

## Global Constraints

- `TreatWarningsAsErrors=true` — any warning fails `dotnet build`
- All handlers return `Result<T>` — no exceptions for control flow
- Vertical slice: `static partial class` split across `GetDashboard.cs`, `GetDashboard.Endpoint.cs`, `GetDashboard.Request.cs`, `GetDashboard.Response.cs`
- Modules in same assembly must not cross-reference directly — Dashboard handler uses `dbContext.Set<T>()` only, no `using Module.Ordering.Domain.Orders.Order` in handler body where it could couple
- `ISoftDeletable.IsDeleted=true` entities excluded from all counts
- No `Domain/` folder — Dashboard has no aggregates, no invariants
- Manager role (already has `DashboardFeatureMetadata.Sales.List`) can access; anonymous → 401; restricted role → 403

## File Structure

```
CREATE: service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs
CREATE: service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Response.cs
CREATE: service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Request.cs
CREATE: service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs
CREATE: service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Endpoint.cs
CREATE: service/Api/tests/Module.UnitTests/Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs
MODIFY: app/Admin/src/features/reports/services/report.service.ts
MODIFY: app/Admin/src/features/reports/stores/report.store.ts
```

---

### Task 1: DashboardFeature route constants and tags

**Files:**
- Create: `service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs`

**Interfaces:**
- Consumes: `DashboardFeatureMetadata.Sales.List` (existing in `Shared/Security/Authorization/Features/`)
- Produces: `DashboardFeature.Admin.Get.Route`, `DashboardFeature.Admin.Get.Permission`, `DashboardFeature.Admin.Get.Summary`, `DashboardFeature.Admin.Get.Description`, `DashboardFeature.Tags.Dashboard`

- [ ] **Step 1: Create the file**

```csharp
// service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs
using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Dashboard.Features.Shared;

public static partial class DashboardFeature
{
    public const string Route = "api/dashboard";

    public static class Tags
    {
        public static readonly string[] Dashboard = ["Dashboard"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = DashboardFeature.Route;
            public const string Description = "Retrieve aggregated dashboard metrics including sales, inventory, catalog, and recent activity";
            public const string Summary = "Get dashboard data";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Sales.List;
        }
    }
}
```

- [ ] **Step 2: Build to verify no compilation errors**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs
git commit -m "feat(dashboard): add route constants and tags for GET /api/dashboard"
```

---

### Task 2: GetDashboard response types

**Files:**
- Create: `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Response.cs`

**Interfaces:**
- Consumes: nothing
- Produces: `GetDashboard.Response`, `SalesSummaryData`, `TrendPoint`, `InventorySummaryData`, `CatalogSummaryData`, `RecentProductData`, `ActivityItemData`

- [ ] **Step 1: Create the file**

```csharp
// service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Response.cs
namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public sealed record Response
    {
        public SalesSummaryData Sales { get; init; } = new();
        public InventorySummaryData Inventory { get; init; } = new();
        public CatalogSummaryData Catalog { get; init; } = new();
        public List<ActivityItemData> RecentActivities { get; init; } = [];
    }

    public sealed record SalesSummaryData
    {
        public decimal TotalRevenue { get; init; }
        public int OrderCount { get; init; }
        public decimal AverageOrderValue { get; init; }
        public decimal RevenueTrendPercentage { get; init; }
        public List<TrendPoint> TrendHistory { get; init; } = [];
    }

    public sealed record TrendPoint(DateOnly Date, decimal Revenue);

    public sealed record InventorySummaryData
    {
        public int TotalVariants { get; init; }
        public int OutOfStockCount { get; init; }
        public int LowStockCount { get; init; }
        public decimal StockAccuracyPercentage { get; init; }
    }

    public sealed record CatalogSummaryData
    {
        public int TotalProducts { get; init; }
        public int ActiveProducts { get; init; }
        public int TotalVariants { get; init; }
        public int TotalTaxonomies { get; init; }
        public int TotalTaxons { get; init; }
        public List<RecentProductData> RecentlyAdded { get; init; } = [];
    }

    public sealed record RecentProductData(Guid Id, string Name, string Slug, DateTime CreatedAtUtc);

    public sealed record ActivityItemData
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }
}
```

- [ ] **Step 2: Build to verify no compilation errors**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Response.cs
git commit -m "feat(dashboard): add GET /api/dashboard response types"
```

---

### Task 3: Request type and Handler

**Files:**
- Create: `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Request.cs`
- Create: `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`, `GetDashboard.Response` types (Task 2)
- Produces: `GetDashboard.Query`, `GetDashboard.QueryHandler`

- [ ] **Step 1: Create Request file**

```csharp
// service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Request.cs
using Shared.Application.Mediators.Queries;

namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public sealed record Query : IQuery<Response>;
}
```

- [ ] **Step 2: Create Handler file**

```csharp
// service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Ordering.Domain.Orders;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
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

        private async Task<SalesSummaryData> BuildSales(
            DateTimeOffset now, DateTimeOffset thirtyDaysAgo, DateTimeOffset sevenDaysAgo,
            CancellationToken ct)
        {
            var baseQuery = dbContext.Set<Order>()
                .Where(o => !o.IsDeleted
                    && o.Status != OrderStatus.Draft
                    && o.Status != OrderStatus.Canceled);

            var totalRevenue = await baseQuery.SumAsync(o => o.Total, ct);
            var orderCount = await baseQuery.CountAsync(ct);

            var sales = new SalesSummaryData
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0m,
            };

            var recentQuery = baseQuery.Where(o => o.CreatedAtUtc >= thirtyDaysAgo);
            var recentRevenue = await recentQuery.SumAsync(o => o.Total, ct);
            var last7Revenue = await recentQuery
                .Where(o => o.CreatedAtUtc >= sevenDaysAgo)
                .SumAsync(o => o.Total, ct);

            var last7Avg = last7Revenue / 7m;
            var previous23Avg = (recentRevenue - last7Revenue) / 23m;
            sales.RevenueTrendPercentage = previous23Avg > 0m
                ? Math.Round((last7Avg / previous23Avg - 1m) * 100m, 2)
                : 0m;

            var thirtyDaysStart = thirtyDaysAgo.Date;
            var dailyRevenue = await recentQuery
                .GroupBy(o => o.CreatedAtUtc.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.Total) })
                .ToListAsync(ct);

            sales.TrendHistory = Enumerable.Range(0, 30)
                .Select(i => thirtyDaysStart.AddDays(i))
                .Select(date =>
                {
                    var entry = dailyRevenue.FirstOrDefault(d => d.Date == date);
                    return new TrendPoint(
                        DateOnly.FromDateTime(date),
                        entry?.Revenue ?? 0m);
                })
                .ToList();

            return sales;
        }

        private async Task<CatalogSummaryData> BuildCatalog(CancellationToken ct)
        {
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
                .Select(p => new RecentProductData(p.Id, p.Name, p.Slug, p.CreatedAtUtc.DateTime))
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

        private async Task<InventorySummaryData> BuildInventory(CancellationToken ct)
        {
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

        private async Task<List<ActivityItemData>> BuildActivities(CancellationToken ct)
        {
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
                    Timestamp = o.CreatedAtUtc.DateTime
                })
                .ToListAsync(ct);

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
                    Timestamp = sm.CreatedAtUtc.DateTime
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
```

- [ ] **Step 3: Build to verify no compilation errors**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

Expected: Build succeeds with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Request.cs \
        service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs
git commit -m "feat(dashboard): add GET /api/dashboard handler with cross-module aggregation"
```

---

### Task 4: Endpoint registration

**Files:**
- Create: `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Endpoint.cs`

**Interfaces:**
- Consumes: `DashboardFeature.Admin.Get` constants (Task 1), `ISender`
- Produces: Carter endpoint at `GET /api/dashboard`

- [ ] **Step 1: Create the file**

```csharp
// service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Endpoint.cs
using Carter;
using MediatR;

using Module.Dashboard.Features.Shared;

using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(DashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetDashboard))
            .WithTags(DashboardFeature.Tags.Dashboard)
            .HasPermission(DashboardFeature.Admin.Get.Permission)
            .WithSummary(DashboardFeature.Admin.Get.Summary)
            .WithDescription(DashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
```

- [ ] **Step 2: Build full solution to verify endpoint is discovered**

```bash
dotnet build
```

Expected: Build succeeds with 0 warnings. Carter auto-discovers `ICarterModule` implementations via `MapCarter()`.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.Endpoint.cs
git commit -m "feat(dashboard): register GET /api/dashboard Carter endpoint"
```

---

### Task 5: Unit tests — Handler logic

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs`

**Interfaces:**
- Consumes: `ApplicationDbContext` (InMemory), `GetDashboard.Query`, `GetDashboard.Response` types
- Produces: 6 passing test cases covering empty DB, orders, products, stock, activities, edge cases

- [ ] **Step 1: Create the test file**

```csharp
// service/Api/tests/Module.UnitTests/Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Dashboard.Features.Admin.Get;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Dashboard.Features.Admin.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Dashboard")]
[Trait("Feature", "GetDashboard")]
public class GetDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetDashboard.QueryHandler _handler;

    public GetDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros and empty lists when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Sales.TotalRevenue.Should().Be(0m);
        response.Sales.OrderCount.Should().Be(0);
        response.Sales.AverageOrderValue.Should().Be(0m);
        response.Sales.RevenueTrendPercentage.Should().Be(0m);
        response.Sales.TrendHistory.Should().HaveCount(30);
        response.Sales.TrendHistory.Should().AllSatisfy(p => p.Revenue.Should().Be(0m));
        response.Inventory.TotalVariants.Should().Be(0);
        response.Inventory.OutOfStockCount.Should().Be(0);
        response.Inventory.LowStockCount.Should().Be(0);
        response.Catalog.TotalProducts.Should().Be(0);
        response.Catalog.ActiveProducts.Should().Be(0);
        response.Catalog.RecentlyAdded.Should().BeEmpty();
        response.RecentActivities.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should compute correct sales totals from placed orders")]
    public async Task Handle_ShouldComputeSales_WhenOrdersExist()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 2,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Placed,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-003",
            Status = OrderStatus.Canceled,
            Total = 99m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var sales = result.Value.Sales;
        sales.TotalRevenue.Should().Be(300m);
        sales.OrderCount.Should().Be(2);
        sales.AverageOrderValue.Should().Be(150m);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted and draft orders from counts")]
    public async Task Handle_ShouldExcludeDeletedAndDraftOrders()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Draft,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-003",
            Status = OrderStatus.Placed,
            Total = 300m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sales.OrderCount.Should().Be(1);
        result.Value.Sales.TotalRevenue.Should().Be(100m);
    }

    [Fact(DisplayName = "Handle: Should count active products, not draft or archived")]
    public async Task Handle_ShouldCountActiveProductsOnly()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Active Product",
            Slug = "active-product",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Draft Product",
            Slug = "draft-product",
            Status = ProductStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Archived Product",
            Slug = "archived-product",
            Status = ProductStatus.Archived,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var catalog = result.Value.Catalog;
        catalog.TotalProducts.Should().Be(3);
        catalog.ActiveProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return recent activities interleaved from orders and stock")]
    public async Task Handle_ShouldReturnInterleavedActivities()
    {
        var ct = TestContext.Current.CancellationToken;

        var orderId = Guid.NewGuid();
        _dbContext.Set<Order>().Add(new Order
        {
            Id = orderId,
            Number = "ORD-100",
            Status = OrderStatus.Placed,
            Total = 50m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-1)
        });

        var locId = Guid.NewGuid();
        var siId = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId,
            Name = "Warehouse A",
            Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = siId,
            VariantId = Guid.NewGuid(),
            StockLocationId = locId,
            CountOnHand = 10
        });
        _dbContext.Set<StockMovement>().Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            StockItemId = siId,
            Quantity = 5,
            Action = "restock",
            Reason = "Shipment received",
            CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-2)
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var activities = result.Value.RecentActivities;
        activities.Should().NotBeEmpty();
        activities.Should().Contain(a => a.Type == "Order");
        activities.Should().Contain(a => a.Type == "Stock");
        activities.Should().BeInDescendingOrder(a => a.Timestamp);
    }

    [Fact(DisplayName = "Handle: TrendHistory should have exactly 30 data points")]
    public async Task Handle_TrendHistory_ShouldHave30Points()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-001",
            Status = OrderStatus.Placed,
            Total = 100m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-15)
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var trend = result.Value.Sales.TrendHistory;
        trend.Should().HaveCount(30);
        trend.Should().BeInAscendingOrder(p => p.Date);
        trend.Should().ContainSingle(p => p.Revenue == 100m);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail or pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Dashboard"
```

Expected: Tests may fail on first run if handler logic needs adjustment. Fix any issues before committing.

- [ ] **Step 3: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Dashboard/
git commit -m "test(dashboard): add unit tests for GET /api/dashboard handler"
```

---

### Task 6: Frontend — wire report service to backend

**Files:**
- Modify: `app/Admin/src/features/reports/services/report.service.ts`

**Interfaces:**
- Consumes: axios client, `GET /api/dashboard` endpoint
- Produces: `reportService.fetchDashboard()` returning `AxiosResponse<Result<DashboardResponse>>`

- [ ] **Step 1: Update report.service.ts**

```typescript
// app/Admin/src/features/reports/services/report.service.ts
import { apiClient } from '@/shared/api/http/api-client'
import type { AxiosResponse } from 'axios'

export interface DashboardResponse {
  sales: {
    totalRevenue: number
    orderCount: number
    averageOrderValue: number
    revenueTrendPercentage: number
    trendHistory: Array<{ date: string; revenue: number }>
  }
  inventory: {
    totalVariants: number
    outOfStockCount: number
    lowStockCount: number
    stockAccuracyPercentage: number
  }
  catalog: {
    totalProducts: number
    activeProducts: number
    totalVariants: number
    totalTaxonomies: number
    totalTaxons: number
    recentlyAdded: Array<{ id: string; name: string; slug: string; createdAtUtc: string }>
  }
  recentActivities: Array<{
    id: string
    type: string
    title: string
    description: string
    status: string
    timestamp: string
  }>
}

export const reportService = {
  fetchDashboard(): Promise<AxiosResponse<{ value: DashboardResponse }>> {
    return apiClient.get('/api/dashboard')
  },
}
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/reports/services/report.service.ts
git commit -m "feat(dashboard): wire report service to GET /api/dashboard"
```

---

### Task 7: Frontend — wire report store to service

**Files:**
- Modify: `app/Admin/src/features/reports/stores/report.store.ts`

**Interfaces:**
- Consumes: `reportService.fetchDashboard()` (Task 6)
- Produces: `useReportStore()` with `sales`, `inventory`, `catalog`, `activities`, `is_loading` refs

- [ ] **Step 1: Update report.store.ts**

```typescript
// app/Admin/src/features/reports/stores/report.store.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { reportService } from '../services/report.service'
import type { SalesSummary, InventorySummary, CatalogSummary, ActivityItem } from '../types/report.types'

export const useReportStore = defineStore('report', () => {
  const sales = ref<SalesSummary | null>(null)
  const inventory = ref<InventorySummary | null>(null)
  const catalog = ref<CatalogSummary | null>(null)
  const activities = ref<ActivityItem[]>([])
  const is_loading = ref(false)

  async function fetchDashboardData() {
    is_loading.value = true
    try {
      const { data } = await reportService.fetchDashboard()
      const value = data.value
      sales.value = { ...value.sales }
      inventory.value = { ...value.inventory }
      catalog.value = { ...value.catalog }
      activities.value = value.recentActivities
    } finally {
      is_loading.value = false
    }
  }

  return { sales, inventory, catalog, activities, is_loading, fetchDashboardData }
})
```

- [ ] **Step 2: Run Admin SPA lint to verify**

```bash
cd app/Admin && pnpm run lint
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/reports/stores/report.store.ts
git commit -m "feat(dashboard): wire report store to service for live dashboard data"
```

---

### Task 8: Build and test — final verification

**Interfaces:**
- Consumes: all backend and frontend files from Tasks 1-7
- Produces: clean build, all tests pass

- [ ] **Step 1: Build full solution**

```bash
dotnet build
```

Expected: 0 errors, 0 warnings.

- [ ] **Step 2: Run Dashboard unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Dashboard"
```

Expected: All 6 tests pass.

- [ ] **Step 3: Run full unit test suite to check for regressions**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All existing tests still pass. No new failures introduced.

- [ ] **Step 4: Run Admin SPA lint to confirm frontend is clean**

```bash
cd app/Admin && pnpm run lint
```

Expected: 0 errors.

- [ ] **Step 5: Verify endpoint appears in Swagger/Scalar docs**

```bash
# Start the API locally (requires Docker for Postgres/Redis via Aspire)
# Then visit http://localhost:5035/scalar/v1 and confirm GET /api/dashboard is listed
```

This step requires a running Docker environment. Skip if local infrastructure is not available.

- [ ] **Step 6: Commit if any fixes were made**

```bash
git add -A
git commit -m "chore(dashboard): final build and test verification"
```

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-16-dashboard-api.md`. Two execution options:

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?
