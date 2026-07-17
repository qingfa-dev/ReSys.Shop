# Per-Module Dashboards Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add per-module dashboard endpoints (Catalog, Ordering, Inventory) with Admin SPA views, complementing the existing global dashboard.

**Architecture:** Each per-module dashboard lives inside its own module as a vertical slice (`Features/Admin/Dashboard/Get/`), queries only its own domain entities, and is gated by a dashboard-scoped permission. The global dashboard (`GET /api/dashboard`) is unchanged.

**Tech Stack:** .NET 10 (C#), Carter minimal APIs, MediatR, EF Core, FluentAssertions, Vue 3 + TypeScript, Pinia, Axios, PrimeVue

## Global Constraints

- All domain operations return `Result<T>` or `Result`
- Modules never reference each other; per-module dashboards query only their own entities
- Vertical slice feature files: `Features/Admin/Dashboard/Get/` with Handler, Request, Response, Endpoint
- `TreatWarningsAsErrors=true` globally
- Frontend follows existing Pinia store + Axios service + Vue view pattern from `features/reports/`

---

## File Structure Map

### C# Backend — New Files

```
service/Api/src/Module/
├── Catalog/Features/
│   ├── Admin/Dashboard/Get/
│   │   ├── GetCatalogDashboard.cs           (handler)
│   │   ├── GetCatalogDashboard.Request.cs
│   │   ├── GetCatalogDashboard.Response.cs
│   │   └── GetCatalogDashboard.Endpoint.cs
│   └── Shared/CatalogDashboardFeature.cs     (route/tag/permission constants)
├── Ordering/Features/
│   ├── Admin/Dashboard/Get/
│   │   ├── GetOrderingDashboard.cs
│   │   ├── GetOrderingDashboard.Request.cs
│   │   ├── GetOrderingDashboard.Response.cs
│   │   └── GetOrderingDashboard.Endpoint.cs
│   └── Shared/OrderingDashboardFeature.cs
└── Inventory/Features/
    ├── Admin/Dashboard/Get/
    │   ├── GetInventoryDashboard.cs
    │   ├── GetInventoryDashboard.Request.cs
    │   ├── GetInventoryDashboard.Response.cs
    │   └── GetInventoryDashboard.Endpoint.cs
    └── Shared/InventoryDashboardFeature.cs
```

### C# Backend — Modified Files

```
service/Api/src/Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs
```

### C# Tests — New Files

```
service/Api/tests/Module.UnitTests/
├── Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboardHandlerTests.cs
├── Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboardHandlerTests.cs
└── Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboardHandlerTests.cs
```

### Admin SPA — New Files

```
app/Admin/src/features/
├── catalog/dashboard/services/catalog-dashboard.service.ts
├── ordering/dashboard/
│   ├── views/OrderingDashboard.View.vue
│   ├── stores/ordering-dashboard.store.ts
│   ├── services/ordering-dashboard.service.ts
│   └── types/ordering-dashboard.types.ts
└── inventories/dashboard/
    ├── views/InventoryDashboard.View.vue
    ├── stores/inventory-dashboard.store.ts
    ├── services/inventory-dashboard.service.ts
    └── types/inventory-dashboard.types.ts
```

### Admin SPA — Modified Files

```
app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts
app/Admin/src/features/ordering/ordering.routes.ts
app/Admin/src/features/inventories/inventory.routes.ts
```

---

### Task 1: Add Permission Metadata

**Files:**
- Modify: `service/Api/src/Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs`

**Interfaces:**
- Produces: `DashboardFeatureMetadata.Catalog.List`, `DashboardFeatureMetadata.Orders.List`, `DashboardFeatureMetadata.Inventory.List` — `PermissionMetadata` instances usable by later tasks

- [ ] **Step 1: Add three new permission classes to DashboardFeatureMetadata.cs**

Open `service/Api/src/Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs`. Add the following three classes after the existing `Logs` class (before the `All` property):

```csharp
    public static class Catalog
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.CatalogDb, PermissionContext.Actions.List);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class Orders
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.Sales, PermissionContext.Actions.List);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class Inventory
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.InventoryDb, PermissionContext.Actions.List);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }
```

Then update the `All` property to include them:

```csharp
    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Sales.All,
        .. InventoryDb.All,
        .. CatalogDb.All,
        .. Activity.All,
        .. Logs.All,
        .. Catalog.All,
        .. Orders.All,
        .. Inventory.All,
    ];
```

- [ ] **Step 2: Build to verify no warnings**

```bash
dotnet build service/Api/src/Shared
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs
git commit -m "feat(dashboard): add Catalog, Orders, Inventory permission metadata"
```

---

### Task 2: Catalog Dashboard — Backend

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Shared/CatalogDashboardFeature.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Request.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Response.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboardHandlerTests.cs`

**Interfaces:**
- Consumes: `DashboardFeatureMetadata.Catalog.List` from Task 1
- Produces: `GET /api/catalog/dashboard` endpoint returning `Result<GetCatalogDashboard.Response>`

**Step 1: Create feature constants file**

Create `service/Api/src/Module/Catalog/Features/Shared/CatalogDashboardFeature.cs`:

```csharp
using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Catalog.Features.Shared;

public static class CatalogDashboardFeature
{
    public const string Route = "api/catalog/dashboard";

    public static class Tags
    {
        public static readonly string[] Catalog = ["Catalog"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = CatalogDashboardFeature.Route;
            public const string Description = "Get catalog dashboard metrics including product, variant, and taxonomy counts";
            public const string Summary = "Get catalog dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Catalog.List;
        }
    }
}
```

- [ ] **Step 2: Create Request**

Create `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Request.cs`:

```csharp
using Shared.Application.Mediators.Queries;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed record Query : IQuery<Response>;
}
```

- [ ] **Step 3: Create Response**

Create `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Response.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed record Response
    {
        public int TotalProducts { get; init; }
        public int ActiveProducts { get; init; }
        public int DraftProducts { get; init; }
        public int TotalVariants { get; init; }
        public int TotalTaxonomies { get; init; }
        public int TotalTaxons { get; init; }
        public List<RecentProductData> RecentProducts { get; init; } = [];
    }

    public sealed record RecentProductData(Guid Id, string Name, string Slug, DateTime CreatedAtUtc);
}
```

*(Note: `DigitalAssetCount` omitted — no `DigitalAsset` entity exists in the Catalog domain. Add when that entity is introduced.)*

- [ ] **Step 4: Create Handler**

Create `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.cs`:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var productsQuery = dbContext.Set<Product>().Where(p => !p.IsDeleted);
            var variantsQuery = dbContext.Set<Variant>().Where(v => !v.IsDeleted);
            var taxonomiesQuery = dbContext.Set<Taxonomy>().Where(t => !t.IsDeleted);
            var taxonsQuery = dbContext.Set<Taxon>().Where(t => !t.IsDeleted);

            var totalProducts = await productsQuery.CountAsync(cancellationToken);
            var activeProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Active, cancellationToken);
            var draftProducts = await productsQuery.CountAsync(p => p.Status == ProductStatus.Draft, cancellationToken);
            var totalVariants = await variantsQuery.CountAsync(cancellationToken);
            var totalTaxonomies = await taxonomiesQuery.CountAsync(cancellationToken);
            var totalTaxons = await taxonsQuery.CountAsync(cancellationToken);

            var recentProducts = await productsQuery
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(5)
                .Select(p => new RecentProductData(p.Id, p.Name, p.Slug, p.CreatedAtUtc.DateTime))
                .ToListAsync(cancellationToken);

            return new Response
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                DraftProducts = draftProducts,
                TotalVariants = totalVariants,
                TotalTaxonomies = totalTaxonomies,
                TotalTaxons = totalTaxons,
                RecentProducts = recentProducts
            };
        }
    }
}
```

- [ ] **Step 5: Create Endpoint**

Create `service/Api/src/Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Endpoint.cs`:

```csharp
using Carter;
using MediatR;
using Module.Catalog.Features.Shared;
using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCatalogDashboard))
            .WithTags(CatalogDashboardFeature.Tags.Catalog)
            .HasPermission(CatalogDashboardFeature.Admin.Get.Permission)
            .WithSummary(CatalogDashboardFeature.Admin.Get.Summary)
            .WithDescription(CatalogDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
```

- [ ] **Step 6: Create Unit Tests**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboardHandlerTests.cs`:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetCatalogDashboard")]
public class GetCatalogDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCatalogDashboard.QueryHandler _handler;

    public GetCatalogDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetCatalogDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalProducts.Should().Be(0);
        response.ActiveProducts.Should().Be(0);
        response.DraftProducts.Should().Be(0);
        response.TotalVariants.Should().Be(0);
        response.TotalTaxonomies.Should().Be(0);
        response.TotalTaxons.Should().Be(0);
        response.RecentProducts.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should count products by status correctly")]
    public async Task Handle_ShouldCountProductsByStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            Slug = "active",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Draft",
            Slug = "draft",
            Status = ProductStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Archived",
            Slug = "archived",
            Status = ProductStatus.Archived,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalProducts.Should().Be(3);
        response.ActiveProducts.Should().Be(1);
        response.DraftProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted products and variants")]
    public async Task Handle_ShouldExcludeDeletedEntities()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Visible",
            Slug = "visible",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Product>().Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Deleted",
            Slug = "deleted",
            Status = ProductStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalProducts.Should().Be(1);
        result.Value.ActiveProducts.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return top 5 recent products ordered by CreatedAtUtc")]
    public async Task Handle_ShouldReturnRecentProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 1; i <= 7; i++)
        {
            _dbContext.Set<Product>().Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Slug = $"product-{i}",
                Status = ProductStatus.Active,
                CreatedAtUtc = baseTime.AddDays(-i),
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var recent = result.Value.RecentProducts;
        recent.Should().HaveCount(5);
        recent.Should().BeInDescendingOrder(p => p.CreatedAtUtc);
    }

    [Fact(DisplayName = "Handle: Should count variants and taxonomies")]
    public async Task Handle_ShouldCountVariantsAndTaxonomies()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Variant>().Add(new Variant
        {
            Id = Guid.NewGuid(),
            IsMaster = true,
            IsDeleted = false
        });
        _dbContext.Set<Variant>().Add(new Variant
        {
            Id = Guid.NewGuid(),
            IsMaster = false,
            IsDeleted = false
        });
        _dbContext.Set<Taxonomy>().Add(new Taxonomy
        {
            Id = Guid.NewGuid(),
            Name = "Categories",
            IsDeleted = false
        });
        _dbContext.Set<Taxon>().Add(new Taxon
        {
            Id = Guid.NewGuid(),
            Name = "Shoes",
            TaxonomyId = Guid.NewGuid(),
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetCatalogDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalVariants.Should().Be(2);
        result.Value.TotalTaxonomies.Should().Be(1);
        result.Value.TotalTaxons.Should().Be(1);
    }
}
```

- [ ] **Step 7: Run unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetCatalogDashboard"
```

Expected: 5 tests pass.

- [ ] **Step 8: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Shared/CatalogDashboardFeature.cs \
        service/Api/src/Module/Catalog/Features/Admin/Dashboard/ \
        service/Api/tests/Module.UnitTests/Catalog/
git commit -m "feat(catalog): add GET /api/catalog/dashboard endpoint with tests"
```

---

### Task 3: Ordering Dashboard — Backend

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Shared/OrderingDashboardFeature.cs`
- Create: `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Request.cs`
- Create: `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Response.cs`
- Create: `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs`
- Create: `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboardHandlerTests.cs`

**Interfaces:**
- Consumes: `DashboardFeatureMetadata.Orders.List` from Task 1
- Produces: `GET /api/ordering/dashboard` endpoint returning `Result<GetOrderingDashboard.Response>`

- [ ] **Step 1: Create feature constants file**

Create `service/Api/src/Module/Ordering/Features/Shared/OrderingDashboardFeature.cs`:

```csharp
using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Ordering.Features.Shared;

public static class OrderingDashboardFeature
{
    public const string Route = "api/ordering/dashboard";

    public static class Tags
    {
        public static readonly string[] Ordering = ["Ordering"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = OrderingDashboardFeature.Route;
            public const string Description = "Get ordering dashboard metrics including order counts, revenue, and status breakdown";
            public const string Summary = "Get ordering dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Orders.List;
        }
    }
}
```

- [ ] **Step 2: Create Request**

Create `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Request.cs`:

```csharp
using Shared.Application.Mediators.Queries;

namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    public sealed record Query : IQuery<Response>;
}
```

- [ ] **Step 3: Create Response**

Create `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Response.cs`:

```csharp
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
```

- [ ] **Step 4: Create Handler**

Create `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs`:

```csharp
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
                .Select(o => new RecentOrderData(o.Id, o.Number, o.Total, o.Status.ToString(), o.CreatedAtUtc.DateTime))
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
```

- [ ] **Step 5: Create Endpoint**

Create `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Endpoint.cs`:

```csharp
using Carter;
using MediatR;
using Module.Ordering.Features.Shared;
using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderingDashboard))
            .WithTags(OrderingDashboardFeature.Tags.Ordering)
            .HasPermission(OrderingDashboardFeature.Admin.Get.Permission)
            .WithSummary(OrderingDashboardFeature.Admin.Get.Summary)
            .WithDescription(OrderingDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
```

- [ ] **Step 6: Create Unit Tests**

Create `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboardHandlerTests.cs`:

```csharp
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Ordering.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderingDashboard")]
public class GetOrderingDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderingDashboard.QueryHandler _handler;

    public GetOrderingDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderingDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalOrders.Should().Be(0);
        response.TotalRevenue.Should().Be(0m);
        response.AverageOrderValue.Should().Be(0m);
        response.PendingFulfillment.Should().Be(0);
        response.TodayOrders.Should().Be(0);
        response.RecentOrders.Should().BeEmpty();
        response.StatusBreakdown.Draft.Should().Be(0);
        response.StatusBreakdown.Placed.Should().Be(0);
        response.StatusBreakdown.Canceled.Should().Be(0);
        response.StatusBreakdown.Expired.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should compute order counts and revenue correctly")]
    public async Task Handle_ShouldComputeOrderCounts_WhenOrdersExist()
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
            Status = OrderStatus.Draft,
            Total = 50m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalOrders.Should().Be(3);
        response.TotalRevenue.Should().Be(350m);
        response.AverageOrderValue.Should().Be(116.67m);
        response.PendingFulfillment.Should().Be(2);
        response.TodayOrders.Should().BeGreaterOrEqualTo(2);
    }

    [Fact(DisplayName = "Handle: Should exclude soft-deleted orders")]
    public async Task Handle_ShouldExcludeDeletedOrders()
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
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Number = "ORD-002",
            Status = OrderStatus.Placed,
            Total = 200m,
            Currency = "USD",
            ItemCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = true
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalOrders.Should().Be(1);
        result.Value.TotalRevenue.Should().Be(100m);
    }

    [Fact(DisplayName = "Handle: Status breakdown should count each status correctly")]
    public async Task Handle_StatusBreakdown_ShouldCountEachStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-1", Status = OrderStatus.Draft,
            Total = 10m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-2", Status = OrderStatus.Placed,
            Total = 20m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-3", Status = OrderStatus.Placed,
            Total = 30m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(), Number = "O-4", Status = OrderStatus.Canceled,
            Total = 40m, Currency = "USD", ItemCount = 1, CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var bd = result.Value.StatusBreakdown;
        bd.Draft.Should().Be(1);
        bd.Placed.Should().Be(2);
        bd.Canceled.Should().Be(1);
        bd.Expired.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should return top 10 recent orders")]
    public async Task Handle_ShouldReturnRecentOrders()
    {
        var ct = TestContext.Current.CancellationToken;
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 1; i <= 12; i++)
        {
            _dbContext.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Number = $"ORD-{i:D3}",
                Status = OrderStatus.Placed,
                Total = i * 10m,
                Currency = "USD",
                ItemCount = 1,
                CreatedAtUtc = baseTime.AddHours(-i)
            });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetOrderingDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var recent = result.Value.RecentOrders;
        recent.Should().HaveCount(10);
        recent.Should().BeInDescendingOrder(o => o.CreatedAtUtc);
    }
}
```

- [ ] **Step 7: Run unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetOrderingDashboard"
```

Expected: 5 tests pass.

- [ ] **Step 8: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Shared/OrderingDashboardFeature.cs \
        service/Api/src/Module/Ordering/Features/Admin/Dashboard/ \
        service/Api/tests/Module.UnitTests/Ordering/
git commit -m "feat(ordering): add GET /api/ordering/dashboard endpoint with tests"
```

---

### Task 4: Inventory Dashboard — Backend

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Shared/InventoryDashboardFeature.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Request.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Response.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboardHandlerTests.cs`

**Interfaces:**
- Consumes: `DashboardFeatureMetadata.Inventory.List` from Task 1
- Produces: `GET /api/inventory/dashboard` endpoint returning `Result<GetInventoryDashboard.Response>`

- [ ] **Step 1: Create feature constants file**

Create `service/Api/src/Module/Inventory/Features/Shared/InventoryDashboardFeature.cs`:

```csharp
using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Inventory.Features.Shared;

public static class InventoryDashboardFeature
{
    public const string Route = "api/inventory/dashboard";

    public static class Tags
    {
        public static readonly string[] Inventory = ["Inventory"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = InventoryDashboardFeature.Route;
            public const string Description = "Get inventory dashboard metrics including stock levels, locations, and recent movements";
            public const string Summary = "Get inventory dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Inventory.List;
        }
    }
}
```

- [ ] **Step 2: Create Request**

Create `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Request.cs`:

```csharp
using Shared.Application.Mediators.Queries;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public sealed record Query : IQuery<Response>;
}
```

- [ ] **Step 3: Create Response**

Create `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Response.cs`:

```csharp
namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public sealed record Response
    {
        public int TotalSkusTracked { get; init; }
        public int InStockCount { get; init; }
        public int OutOfStockCount { get; init; }
        public int LowStockCount { get; init; }
        public int StockLocationCount { get; init; }
        public int ItemsPerLocationAverage { get; init; }
        public List<RecentMovementData> RecentMovements { get; init; } = [];
    }

    public sealed record RecentMovementData(
        Guid Id, int Quantity, string? Action, string? Reason, DateTime CreatedAtUtc);
}
```

- [ ] **Step 4: Create Handler**

Create `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.cs`:

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Shared.Application.Mediators.Queries;
using Shared.Operational.Persistence.Data;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
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
                .Where(sm => locationIds.Contains(sm.StockLocationId ?? Guid.Empty) || sm.StockLocationId == null)
                .OrderByDescending(sm => sm.CreatedAtUtc)
                .Take(10)
                .Select(sm => new RecentMovementData(
                    sm.Id, sm.Quantity, sm.Action, sm.Reason, sm.CreatedAtUtc.DateTime))
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
```

- [ ] **Step 5: Create Endpoint**

Create `service/Api/src/Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Endpoint.cs`:

```csharp
using Carter;
using MediatR;
using Module.Inventory.Features.Shared;
using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetInventoryDashboard))
            .WithTags(InventoryDashboardFeature.Tags.Inventory)
            .HasPermission(InventoryDashboardFeature.Admin.Get.Permission)
            .WithSummary(InventoryDashboardFeature.Admin.Get.Summary)
            .WithDescription(InventoryDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
```

- [ ] **Step 6: Create Unit Tests**

Create `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboardHandlerTests.cs`:

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Features.Admin.Dashboard.Get;

namespace Module.UnitTests.Inventory.Features.Admin.Dashboard.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetInventoryDashboard")]
public class GetInventoryDashboardHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetInventoryDashboard.QueryHandler _handler;

    public GetInventoryDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetInventoryDashboard.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return all zeros when database is empty")]
    public async Task Handle_ShouldReturnEmpty_WhenDatabaseIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(0);
        response.InStockCount.Should().Be(0);
        response.OutOfStockCount.Should().Be(0);
        response.LowStockCount.Should().Be(0);
        response.StockLocationCount.Should().Be(0);
        response.ItemsPerLocationAverage.Should().Be(0);
        response.RecentMovements.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should count stock levels correctly")]
    public async Task Handle_ShouldCountStockLevels()
    {
        var ct = TestContext.Current.CancellationToken;
        var locId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId, Name = "Warehouse", Active = true, LowStockThreshold = 5
        });

        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 10
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 0
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = locId, CountOnHand = 3
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(3);
        response.InStockCount.Should().Be(2);
        response.OutOfStockCount.Should().Be(1);
        response.LowStockCount.Should().Be(1);
        response.StockLocationCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should exclude inactive and deleted locations")]
    public async Task Handle_ShouldExcludeInactiveLocations()
    {
        var ct = TestContext.Current.CancellationToken;
        var activeId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = activeId, Name = "Active", Active = true
        });
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = inactiveId, Name = "Inactive", Active = false
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = activeId, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = inactiveId, CountOnHand = 5
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.TotalSkusTracked.Should().Be(1);
        response.StockLocationCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return recent stock movements")]
    public async Task Handle_ShouldReturnRecentMovements()
    {
        var ct = TestContext.Current.CancellationToken;
        var locId = Guid.NewGuid();
        var siId = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = locId, Name = "Warehouse", Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = siId, VariantId = Guid.NewGuid(), StockLocationId = locId, CountOnHand = 10
        });
        _dbContext.Set<StockMovement>().Add(new StockMovement
        {
            Id = Guid.NewGuid(), StockItemId = siId, Quantity = 5,
            Action = "restock", Reason = "Shipment", CreatedAtUtc = DateTimeOffset.UtcNow,
            StockLocationId = locId
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecentMovements.Should().HaveCount(1);
        result.Value.RecentMovements[0].Action.Should().Be("restock");
        result.Value.RecentMovements[0].Quantity.Should().Be(5);
    }

    [Fact(DisplayName = "Handle: ItemsPerLocationAverage should be correct")]
    public async Task Handle_ItemsPerLocationAverage_ShouldBeCorrect()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc1Id = Guid.NewGuid();
        var loc2Id = Guid.NewGuid();

        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = loc1Id, Name = "WH1", Active = true
        });
        _dbContext.Set<StockLocation>().Add(new StockLocation
        {
            Id = loc2Id, Name = "WH2", Active = true
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc1Id, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc1Id, CountOnHand = 5
        });
        _dbContext.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(), VariantId = Guid.NewGuid(),
            StockLocationId = loc2Id, CountOnHand = 5
        });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetInventoryDashboard.Query(), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.ItemsPerLocationAverage.Should().Be(2);
    }
}
```

- [ ] **Step 7: Run unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetInventoryDashboard"
```

Expected: 5 tests pass.

- [ ] **Step 8: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Shared/InventoryDashboardFeature.cs \
        service/Api/src/Module/Inventory/Features/Admin/Dashboard/ \
        service/Api/tests/Module.UnitTests/Inventory/
git commit -m "feat(inventory): add GET /api/inventory/dashboard endpoint with tests"
```

---

### Task 5: Catalog Dashboard — Frontend (Wire Stub)

**Files:**
- Create: `app/Admin/src/features/catalog/dashboard/services/catalog-dashboard.service.ts`
- Modify: `app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts`
- Modify: `app/Admin/src/features/catalog/dashboard/types/catalog-dashboard.types.ts` (if needed)

**Interfaces:**
- Consumes: `GET /api/catalog/dashboard` from Task 2
- Produces: Live catalog dashboard view at `/catalog`

- [ ] **Step 1: Create service**

Create `app/Admin/src/features/catalog/dashboard/services/catalog-dashboard.service.ts`:

```ts
import apiClient from '@/shared/api/http/api.client'
import type { AxiosResponse } from 'axios'

export interface CatalogDashboardResponse {
  totalProducts: number
  activeProducts: number
  draftProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentProducts: Array<{
    id: string
    name: string
    slug: string
    createdAtUtc: string
  }>
}

export const catalogDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<CatalogDashboardResponse>> {
    return apiClient.get('/catalog/dashboard')
  },
}
```

- [ ] **Step 2: Update store**

Replace the stub `fetchSummary()` in `app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts`:

```ts
import { defineStore } from 'pinia';
import { ref } from 'vue';
import { catalogDashboardService } from '../services/catalog-dashboard.service';
import type { CatalogDashboardResponse } from '../services/catalog-dashboard.service';

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref<CatalogDashboardResponse | null>(null);
  const loading = ref(false);

  async function fetchSummary() {
    loading.value = true;
    try {
      const { data } = await catalogDashboardService.fetchDashboard();
      summary.value = { ...data };
    } finally {
      loading.value = false;
    }
  }

  return { summary, loading, fetchSummary };
});
```

- [ ] **Step 3: Verify the view works**

The existing `CatalogDashboard.View.vue` already renders stat cards using `summary` from the store. Once the store is wired, it should display real data. Quick check that field names in the view match the API response. The view uses `summary.totalProducts`, `summary.activeProducts`, `summary.totalVariants`, `summary.totalTaxonomies`, `summary.totalTaxons`, `summary.recentlyAdded` — note the view uses `recentlyAdded` (camelCase from the old type) but the API returns `recentProducts`. Update the view mapping if needed:

```bash
# Check the view's data bindings
grep -n 'recentlyAdded\|recentProducts\|totalDigitalProducts' app/Admin/src/features/catalog/dashboard/views/CatalogDashboard.View.vue
```

If the view references `summary.recentlyAdded`, update it to `summary.recentProducts`. If it references `totalDigitalProducts`, remove that stat card (no backend entity exists).

- [ ] **Step 4: Run Admin SPA lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: No lint errors.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/dashboard/
git commit -m "feat(admin): wire catalog dashboard store to GET /api/catalog/dashboard"
```

---

### Task 6: Ordering Dashboard — Frontend

**Files:**
- Create: `app/Admin/src/features/ordering/dashboard/types/ordering-dashboard.types.ts`
- Create: `app/Admin/src/features/ordering/dashboard/services/ordering-dashboard.service.ts`
- Create: `app/Admin/src/features/ordering/dashboard/stores/ordering-dashboard.store.ts`
- Create: `app/Admin/src/features/ordering/dashboard/views/OrderingDashboard.View.vue`
- Modify: `app/Admin/src/features/ordering/ordering.routes.ts`

**Interfaces:**
- Consumes: `GET /api/ordering/dashboard` from Task 3
- Produces: Ordering dashboard view at `/ordering`

- [ ] **Step 1: Create types**

Create `app/Admin/src/features/ordering/dashboard/types/ordering-dashboard.types.ts`:

```ts
export interface RecentOrder {
  id: string
  number: string
  total: number
  status: string
  createdAtUtc: string
}

export interface OrderStatusBreakdown {
  draft: number
  placed: number
  canceled: number
  expired: number
}

export interface OrderingDashboardResponse {
  totalOrders: number
  pendingFulfillment: number
  todayOrders: number
  averageOrderValue: number
  totalRevenue: number
  recentOrders: RecentOrder[]
  statusBreakdown: OrderStatusBreakdown
}
```

- [ ] **Step 2: Create service**

Create `app/Admin/src/features/ordering/dashboard/services/ordering-dashboard.service.ts`:

```ts
import apiClient from '@/shared/api/http/api.client'
import type { AxiosResponse } from 'axios'
import type { OrderingDashboardResponse } from '../types/ordering-dashboard.types'

export const orderingDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<OrderingDashboardResponse>> {
    return apiClient.get('/ordering/dashboard')
  },
}
```

- [ ] **Step 3: Create store**

Create `app/Admin/src/features/ordering/dashboard/stores/ordering-dashboard.store.ts`:

```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { orderingDashboardService } from '../services/ordering-dashboard.service'
import type { OrderingDashboardResponse } from '../types/ordering-dashboard.types'

export const useOrderingDashboardStore = defineStore('ordering-dashboard', () => {
  const data = ref<OrderingDashboardResponse | null>(null)
  const loading = ref(false)

  async function fetchDashboard() {
    loading.value = true
    try {
      const { data: response } = await orderingDashboardService.fetchDashboard()
      data.value = { ...response }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, fetchDashboard }
})
```

- [ ] **Step 4: Create view**

Create `app/Admin/src/features/ordering/dashboard/views/OrderingDashboard.View.vue`:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useOrderingDashboardStore } from '../stores/ordering-dashboard.store'
import { storeToRefs } from 'pinia'

const store = useOrderingDashboardStore()
const { data, loading } = storeToRefs(store)

onMounted(async () => {
  await store.fetchDashboard()
})
</script>

<template>
  <div class="p-6">
    <div class="mb-8">
      <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
        Ordering Dashboard
      </h2>
      <p class="text-surface-500 dark:text-surface-400">
        Overview of orders, revenue, and fulfillment.
      </p>
    </div>

    <div v-if="loading && !data" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
      <Skeleton v-for="i in 5" :key="i" height="100px" class="rounded-2xl" />
    </div>

    <div v-else-if="data" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Total Orders</p>
        <p class="text-3xl font-bold mt-2">{{ data.totalOrders.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Pending Fulfillment</p>
        <p class="text-3xl font-bold mt-2">{{ data.pendingFulfillment.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Today's Orders</p>
        <p class="text-3xl font-bold mt-2">{{ data.todayOrders.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Avg. Order Value</p>
        <p class="text-3xl font-bold mt-2">${{ data.averageOrderValue.toFixed(2) }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Total Revenue</p>
        <p class="text-3xl font-bold mt-2">${{ data.totalRevenue.toLocaleString() }}</p>
      </div>
    </div>

    <div v-if="data" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2">
        <h3 class="text-lg font-semibold mb-3">Recent Orders</h3>
        <DataTable :value="data.recentOrders" class="text-sm" stripedRows>
          <Column field="number" header="Order #" />
          <Column field="status" header="Status" />
          <Column field="total" header="Total">
            <template #body="{ data: row }">${{ row.total.toFixed(2) }}</template>
          </Column>
          <Column field="createdAtUtc" header="Date">
            <template #body="{ data: row }">{{ new Date(row.createdAtUtc).toLocaleDateString() }}</template>
          </Column>
        </DataTable>
      </div>
      <div>
        <h3 class="text-lg font-semibold mb-3">Status Breakdown</h3>
        <div class="space-y-2">
          <div class="flex justify-between text-sm">
            <span>Draft</span>
            <span class="font-semibold">{{ data.statusBreakdown.draft }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>Placed</span>
            <span class="font-semibold">{{ data.statusBreakdown.placed }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>Canceled</span>
            <span class="font-semibold">{{ data.statusBreakdown.canceled }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>Expired</span>
            <span class="font-semibold">{{ data.statusBreakdown.expired }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 5: Add route**

Modify `app/Admin/src/features/ordering/ordering.routes.ts` — add a dashboard child route at the root:

```ts
import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  meta: { breadcrumb: 'Orders' },
  children: [
    {
      path: '',
      name: 'ordering.dashboard',
      component: () => import('./dashboard/views/OrderingDashboard.View.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/views/OrderList.View.vue'),
    },
    {
      path: 'orders/create',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/views/OrderForm.View.vue'),
      meta: { breadcrumb: 'Create Order' },
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.detail',
      component: () => import('@/features/ordering/views/OrderDetail.View.vue'),
      meta: { breadcrumb: 'Detail' },
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/fulfillment/views/FulfillmentQueue.View.vue'),
      meta: { breadcrumb: 'Fulfillment' },
    },
  ],
}
```

- [ ] **Step 6: Run Admin SPA lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: No lint errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/ordering/
git commit -m "feat(admin): add ordering dashboard with store, service, and view"
```

---

### Task 7: Inventory Dashboard — Frontend

**Files:**
- Create: `app/Admin/src/features/inventories/dashboard/types/inventory-dashboard.types.ts`
- Create: `app/Admin/src/features/inventories/dashboard/services/inventory-dashboard.service.ts`
- Create: `app/Admin/src/features/inventories/dashboard/stores/inventory-dashboard.store.ts`
- Create: `app/Admin/src/features/inventories/dashboard/views/InventoryDashboard.View.vue`
- Modify: `app/Admin/src/features/inventories/inventory.routes.ts`

**Interfaces:**
- Consumes: `GET /api/inventory/dashboard` from Task 4
- Produces: Inventory dashboard view at `/inventory`

- [ ] **Step 1: Create types**

Create `app/Admin/src/features/inventories/dashboard/types/inventory-dashboard.types.ts`:

```ts
export interface RecentMovement {
  id: string
  quantity: number
  action: string | null
  reason: string | null
  createdAtUtc: string
}

export interface InventoryDashboardResponse {
  totalSkusTracked: number
  inStockCount: number
  outOfStockCount: number
  lowStockCount: number
  stockLocationCount: number
  itemsPerLocationAverage: number
  recentMovements: RecentMovement[]
}
```

- [ ] **Step 2: Create service**

Create `app/Admin/src/features/inventories/dashboard/services/inventory-dashboard.service.ts`:

```ts
import apiClient from '@/shared/api/http/api.client'
import type { AxiosResponse } from 'axios'
import type { InventoryDashboardResponse } from '../types/inventory-dashboard.types'

export const inventoryDashboardService = {
  fetchDashboard(): Promise<AxiosResponse<InventoryDashboardResponse>> {
    return apiClient.get('/inventory/dashboard')
  },
}
```

- [ ] **Step 3: Create store**

Create `app/Admin/src/features/inventories/dashboard/stores/inventory-dashboard.store.ts`:

```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { inventoryDashboardService } from '../services/inventory-dashboard.service'
import type { InventoryDashboardResponse } from '../types/inventory-dashboard.types'

export const useInventoryDashboardStore = defineStore('inventory-dashboard', () => {
  const data = ref<InventoryDashboardResponse | null>(null)
  const loading = ref(false)

  async function fetchDashboard() {
    loading.value = true
    try {
      const { data: response } = await inventoryDashboardService.fetchDashboard()
      data.value = { ...response }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, fetchDashboard }
})
```

- [ ] **Step 4: Create view**

Create `app/Admin/src/features/inventories/dashboard/views/InventoryDashboard.View.vue`:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useInventoryDashboardStore } from '../stores/inventory-dashboard.store'
import { storeToRefs } from 'pinia'

const store = useInventoryDashboardStore()
const { data, loading } = storeToRefs(store)

onMounted(async () => {
  await store.fetchDashboard()
})
</script>

<template>
  <div class="p-6">
    <div class="mb-8">
      <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
        Inventory Dashboard
      </h2>
      <p class="text-surface-500 dark:text-surface-400">
        Stock levels, locations, and recent movements.
      </p>
    </div>

    <div v-if="loading && !data" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6 mb-8">
      <Skeleton v-for="i in 6" :key="i" height="100px" class="rounded-2xl" />
    </div>

    <div v-else-if="data" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6 mb-8">
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">SKUs Tracked</p>
        <p class="text-3xl font-bold mt-2">{{ data.totalSkusTracked.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">In Stock</p>
        <p class="text-3xl font-bold mt-2 text-green-600">{{ data.inStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Out of Stock</p>
        <p class="text-3xl font-bold mt-2 text-red-500">{{ data.outOfStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Low Stock</p>
        <p class="text-3xl font-bold mt-2 text-orange-500">{{ data.lowStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Locations</p>
        <p class="text-3xl font-bold mt-2">{{ data.stockLocationCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Avg Items/Location</p>
        <p class="text-3xl font-bold mt-2">{{ data.itemsPerLocationAverage.toLocaleString() }}</p>
      </div>
    </div>

    <div v-if="data">
      <h3 class="text-lg font-semibold mb-3">Recent Stock Movements</h3>
      <DataTable :value="data.recentMovements" class="text-sm" stripedRows>
        <Column field="action" header="Action" />
        <Column field="quantity" header="Qty" />
        <Column field="reason" header="Reason" />
        <Column field="createdAtUtc" header="Date">
          <template #body="{ data: row }">
            {{ new Date(row.createdAtUtc).toLocaleDateString() }}
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
```

- [ ] **Step 5: Add route**

Modify `app/Admin/src/features/inventories/inventory.routes.ts` — add a dashboard child route at the root:

```ts
import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  meta: { breadcrumb: 'Inventory' },
  children: [
    {
      path: '',
      name: 'inventory.dashboard',
      component: () => import('./dashboard/views/InventoryDashboard.View.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('./views/StockItemList.View.vue'),
      meta: { breadcrumb: 'Stock Levels' }
    },
    {
      path: 'units',
      name: 'inventory.units.list',
      component: () => import('./views/InventoryUnitList.View.vue'),
      meta: { breadcrumb: 'Serialized Units' }
    },
    {
      path: 'locations',
      meta: { breadcrumb: 'Warehouses' },
      component: () => import('./views/StockLocationManager.View.vue'),
      children: [
        {
          path: '',
          name: 'inventory.locations.list',
          component: () => import('./views/StockLocationList.View.vue'),
        },
        {
          path: 'create',
          name: 'inventory.locations.create',
          component: () => import('./views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Add Location' }
        },
        {
          path: ':id/edit',
          name: 'inventory.locations.edit',
          component: () => import('./views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Edit Location' }
        },
      ]
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('./views/StockTransferList.View.vue'),
      meta: { breadcrumb: 'Logistics' }
    },
    {
      path: 'transfers/create',
      name: 'inventory.transfers.create',
      component: () => import('./views/StockTransferForm.View.vue'),
      meta: { breadcrumb: 'Initiate Transfer' }
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.detail',
      component: () => import('./views/StockTransferDetail.View.vue'),
      meta: { breadcrumb: 'Transfer Details' }
    }
  ]
}
```

- [ ] **Step 6: Run Admin SPA lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: No lint errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/inventories/
git commit -m "feat(admin): add inventory dashboard with store, service, and view"
```

---

### Task 8: Build and Verify All

**Files:**
- None (verify-only task)

- [ ] **Step 1: Full .NET build with warnings-as-errors**

```bash
dotnet build
```

Expected: Build succeeded with 0 warnings.

- [ ] **Step 2: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All tests pass (existing + 15 new).

- [ ] **Step 3: Verify Admin SPA builds**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeded with no errors.

- [ ] **Step 4: Commit (if anything changed)**

```bash
git status
```

If clean, done. If any build fixes were applied, `git add` and commit them.

---
