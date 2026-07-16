---
title: Admin Dashboard API — GET /api/dashboard
version: 1.0
date_created: 2026-07-16
owner: Platform Team
tags: design, api, admin, dashboard
---

# Admin Dashboard API

Single aggregated endpoint powering the Admin SPA dashboard at `reports/dashboard`. Returns pre-computed sales stats, inventory health, catalog snapshot, and recent activity in one response.

## 1. Purpose & Scope

Define the backend `GET /api/dashboard` endpoint that feeds the existing Admin SPA dashboard view (`app/Admin/src/features/reports/views/dashboard.view.vue`). The frontend types (`report.types.ts`), store (`report.store.ts`), and UI are already built — they need data. This spec covers only the backend feature; frontend wiring is documented in §8 for completeness.

Audience: API developers implementing the Dashboard feature.

## 2. Definitions

| Term | Definition |
|------|-----------|
| Dashboard | Read-only aggregation endpoint that queries across Catalog, Ordering, and Inventory modules |
| Vertical slice | Single `static partial class` split across files: Handler, Request, Response, Endpoint |
| `DashboardFeatureMetadata` | Existing permission definitions in `Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs` |
| `ISender` | MediatR interface for sending queries/commands between modules |
| `IApplicationDbContext` | EF Core DbContext abstraction — single context across all Module DbSets |
| `Result<T>` | Domain result object — all handlers return this |
| `ProductStatus.Active` | Enum value `1` for active products |

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: `GET /api/dashboard` returns a single JSON response with four sections: `SalesSummary`, `InventorySummary`, `CatalogSummary`, `RecentActivities`.
- **REQ-002**: Response shape maps 1:1 to existing frontend types in `report.types.ts`. No translation layer needed.
- **REQ-003**: Handler uses `IApplicationDbContext` directly for read-only queries. No new repository, service, or persistence layer.
- **REQ-004**: Permission gate via `DashboardFeatureMetadata.Sales.List` (existing). Manager role already includes this permission.
- **REQ-005**: Endpoint returns `200 OK` with `Result<Response>`. On partial failure (e.g., one module query times out), return degraded result with zeroed values — never 500.
- **CON-001**: No date range filtering in MVP. All aggregates hardcode "last 30 days" for trend history, all-time for totals.
- **CON-002**: No `Domain/` folder. Dashboard is a read-only cross-cutting feature with no entities, no aggregates, no persistence.
- **CON-003**: Handler must not reference module-internal domain classes directly (e.g., `Order`). Use `dbContext.Set<Order>()` via DbSet or named EF queries. Rationale: loose coupling so modules can evolve their domain models independently.
- **CON-004**: Feature files follow vertical slice naming: `GetDashboard.{Role}.cs`.
- **PAT-001**: Route constants, tags, and permission metadata follow the `InventoryFeature.Admin.*` pattern.

## 4. Interfaces & Data Contracts

### 4.1 Route

```
GET /api/dashboard
```

No query parameters in MVP. No path parameters.

### 4.2 Response

```csharp
// GetDashboard.Response.cs
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
        public string Type { get; init; } = string.Empty;   // "Order" | "Stock"
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }
}
```

### 4.3 Request

```csharp
// GetDashboard.Request.cs
namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public sealed record Query : IQuery<Response>;
}
```

### 4.4 Query Sources

| Response field | Source entity | Query |
|---|---|---|
| `Sales.TotalRevenue` | `Ordering.Order` | `SUM(Total)` where `Status != Draft AND Status != Canceled` |
| `Sales.OrderCount` | `Ordering.Order` | `COUNT(*)` where `Status != Draft AND Status != Canceled` |
| `Sales.AverageOrderValue` | computed | `TotalRevenue / OrderCount` (decimal division) |
| `Sales.RevenueTrendPercentage` | computed | `(last-30d avg / last-7d avg - 1) * 100`, or `0` if no history |
| `Sales.TrendHistory` | `Ordering.Order` | `GROUP BY DATE(CreatedAtUtc)`, SUM(Total), last 30d, ordered ASC |
| `Inventory.TotalVariants` | `Catalog.Variant` | `COUNT(*)` where `!IsDeleted` |
| `Inventory.OutOfStockCount` | `Inventory.StockItem` | `COUNT(DISTINCT VariantId)` where `SUM(CountOnHand) = 0` per variant |
| `Inventory.LowStockCount` | `Inventory.StockItem` + `StockLocation` | Count variants where `SUM(CountOnHand) <= threshold` (join `StockLocation.LowStockThreshold`) |
| `Inventory.StockAccuracyPercentage` | placeholder | `100` (no audit system yet — defer to future cycle count feature) |
| `Catalog.TotalProducts` | `Catalog.Product` | `COUNT(*)` where `!IsDeleted` |
| `Catalog.ActiveProducts` | `Catalog.Product` | `COUNT(*)` where `!IsDeleted AND Status == ProductStatus.Active` |
| `Catalog.TotalVariants` | `Catalog.Variant` | `COUNT(*)` where `!IsDeleted` |
| `Catalog.TotalTaxonomies` | `Catalog.Taxonomy` | `COUNT(*)` where `!IsDeleted` (or `dbContext.Set<Taxonomy>()`) |
| `Catalog.TotalTaxons` | `Catalog.Taxon` | `COUNT(*)` where `!IsDeleted` |
| `Catalog.RecentlyAdded` | `Catalog.Product` | TOP 5 where `!IsDeleted`, ordered by `CreatedAtUtc DESC` |
| `RecentActivities` | `Ordering.Order` UNION `Inventory.StockMovement` | TOP 20 events, interleaved, by `CreatedAtUtc DESC` |

### 4.5 Activity Item Construction

```
For each Order (last 20):
  Type = "Order"
  Title = "Order #{Number}"
  Description = "{ItemCount} item(s) · {Currency} {Total}"
  Status = Status.ToString()
  Timestamp = CreatedAtUtc

For each StockMovement (last 20):
  Type = "Stock"
  Title = "Stock: {variant name or SKU}"
  Description = "{Quantity} units moved at {LocationName}"
  Status = MovementType or "Completed"
  Timestamp = CreatedAtUtc

Unified: take last 20 from each source, merge, sort by Timestamp DESC, take top 20.
```

### 4.6 Route Constants

```csharp
// DashboardFeature.Admin.cs (new file, follows InventoryFeature pattern)
namespace Module.Dashboard.Features.Shared;

public static partial class DashboardFeature
{
    public const string Route = "api/dashboard";

    public static class Admin
    {
        public static class Get
        {
            public const string Route = DashboardFeature.Route;
            public const string Description = "Retrieve aggregated dashboard metrics";
            public const string Summary = "Get dashboard data";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Sales.List;
        }
    }
}
```

### 4.7 Endpoint

```csharp
// GetDashboard.Endpoint.cs
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
            .WithTags("Dashboard")
            .HasPermission(DashboardFeature.Admin.Get.Permission)
            .WithSummary(DashboardFeature.Admin.Get.Summary)
            .WithDescription(DashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
```

## 5. Acceptance Criteria

- **AC-001**: `GET /api/dashboard` returns `200 OK` with `Result<Response>` containing all four sections when data exists.
- **AC-002**: Response returns `200 OK` with zeroed values (`TotalRevenue=0`, `OrderCount=0`, empty lists) when no orders/products/stock exist. No 500.
- **AC-003**: `TrendHistory` contains up to 30 daily data points, ordered by date ASC.
- **AC-004**: `RecentActivities` contains at most 20 items, sorted by `Timestamp` DESC, interleaving Orders and StockMovements.
- **AC-005**: Manager role can access the endpoint. Unauthenticated requests return `401`. Non-Manager authenticated users return `403`.
- **AC-006**: `AverageOrderValue` returns `decimal`, computed as `TotalRevenue / OrderCount` when `OrderCount > 0`, `0` otherwise.
- **AC-007**: Handler returns `Result<Response>` — no raw exceptions reach the endpoint layer.
- **AC-008**: Soft-deleted entities (`IsDeleted=true`) are excluded from all counts.

## 6. Test Automation Strategy

- **Test Levels**: Unit (handler + response mapping), Integration (endpoint + real DbContext)
- **Frameworks**: MSTest, FluentAssertions, NSubstitute (for ISender in unit tests), Testcontainers (PostgreSQL for integration)
- **Test Data Management**: Seed test data via `TestDataSeeder` or inline entity creation in integration tests
- **CI/CD**: `dotnet test --filter "FullyQualifiedName~Dashboard"` — included in existing CI pipeline
- **Coverage**: Unit test covers handler logic with mocked `IApplicationDbContext`. Integration test covers endpoint with real DB.
- **Key test cases**:
  1. Empty database → all zeros, empty lists, 200 OK
  2. Orders present → `TotalRevenue`, `OrderCount`, `AverageOrderValue` computed correctly
  3. Products with `Status=Active` → `ActiveProducts` count only active, not Draft/Archived
  4. Soft-deleted entities excluded from all counts
  5. `TrendHistory` spans exactly 30 days, ordered ASC
  6. `RecentActivities` interleaves Order and Stock items, max 20, ordered DESC by Timestamp
  7. Unauthorized request → 401
  8. Manager role request → 200
  9. Non-Manager role request → 403

## 7. Rationale & Context

**Why one endpoint, not multiple:** The frontend already calls a single `fetchDashboardData()`. Dashboard data is <100KB, loaded once per admin session. Splitting into per-section endpoints adds N+1 latency for no benefit at this scale.

**Why no Domain/ folder:** Dashboard has no aggregate root, no invariants, no business logic. It is a pure read-model projection. Adding a `Domain/` folder would violate YAGNI and bloat the Module with an empty layer.

**Why direct DbContext, not MediatR queries to each module:** The alternative (adding internal query handlers in Catalog, Ordering, Inventory modules) is architecturally cleaner but adds ~15 files and 3 cross-module round-trips for a single dashboard load. Start pragmatic; refactor to per-module handlers when module isolation becomes a real problem (e.g., when modules split into separate assemblies).

**Why hardcoded 30-day window:** The UI shows "Revenue Trend (30 Days)" — no date picker exists. Adding `from`/`to` query params is a future enhancement. Scope locked to what the UI renders today.

**Why `DashboardFeatureMetadata.Sales.List` for the gate:** The existing 5 permissions (`Sales.List`, `InventoryDb.List`, `CatalogDb.List`, `Activity.List`, `Logs.Audit`) are too granular for an aggregated dashboard. `Sales.List` is the primary permission; the Manager role already includes it. Fine-grained per-section gates are deferred until per-section widgets exist.

**Why `IApplicationDbContext` instead of raw SQL:** The Module assembly uses EF Core as the single source of truth for entity definitions. Raw SQL (Dapper) bypasses the entity configuration (soft-delete filters, value converters, enum mappings) and would duplicate schema knowledge.

## 8. Dependencies & External Integrations

### Feature Files to Create

```
service/Api/src/Module/Dashboard/
  Features/
    Shared/
      DashboardFeature.cs              # route constants + tags (new)
    Admin/
      Get/
        GetDashboard.cs                # Handler (new)
        GetDashboard.Request.cs        # Query record (new)
        GetDashboard.Response.cs       # Nested response records (new)
        GetDashboard.Endpoint.cs       # Carter ICarterModule (new)
```

### Files to Modify (Frontend)

| File | Change |
|------|--------|
| `app/Admin/src/features/reports/services/report.service.ts` | Add `axios.get('/api/dashboard')` |
| `app/Admin/src/features/reports/stores/report.store.ts` | Wire service call in `fetchDashboardData()`, map to 4 refs |
| `app/Admin/src/features/reports/types/report.types.ts` | No change needed — types already match response shape |

### Files to Modify (Backend)

| File | Change |
|------|--------|
| `PermissionContext.cs:L234` | Already includes `DashboardFeatureMetadata.All` — no change needed |
| `Role.Constant.cs:L84` | Already includes Dashboard permissions for Manager — no change needed |

### Technology Dependencies

- EF Core (IApplicationDbContext) — already registered in DI
- MediatR (ISender) — already registered
- Carter (ICarterModule) — endpoint auto-discovered by `MapCarter()`
- `DashboardFeatureMetadata` — already exists in `Shared/Security/Authorization/Features/`

## 9. Examples & Edge Cases

### Normal response

```json
{
  "value": {
    "sales": {
      "totalRevenue": 154230.50,
      "orderCount": 847,
      "averageOrderValue": 182.09,
      "revenueTrendPercentage": 12.5,
      "trendHistory": [
        { "date": "2026-06-16", "revenue": 4520.00 },
        { "date": "2026-06-17", "revenue": 5180.00 }
      ]
    },
    "inventory": {
      "totalVariants": 3200,
      "outOfStockCount": 45,
      "lowStockCount": 120,
      "stockAccuracyPercentage": 100.0
    },
    "catalog": {
      "totalProducts": 850,
      "activeProducts": 720,
      "totalVariants": 3200,
      "totalTaxonomies": 12,
      "totalTaxons": 145,
      "recentlyAdded": [
        { "id": "guid", "name": "Summer Dress", "slug": "summer-dress", "createdAtUtc": "2026-07-15T10:30:00Z" }
      ]
    },
    "recentActivities": [
      { "id": "guid", "type": "Order", "title": "Order #ORD-1005", "description": "3 item(s) · USD 245.00", "status": "Placed", "timestamp": "2026-07-16T08:22:00Z" },
      { "id": "guid", "type": "Stock", "title": "Stock: SKU-SM-001", "description": "50 units moved at Main Warehouse", "status": "Completed", "timestamp": "2026-07-16T07:15:00Z" }
    ]
  }
}
```

### Edge cases

**Empty database:** All numeric fields `0`, all lists empty `[]`. `200 OK` — never `500` or error result.

**Division by zero:** `AverageOrderValue = 0m` when `OrderCount == 0`.

**RevenueTrendPercentage with no prior data:** Return `0` when less than 7 days of history exist (trend is not computable).

**Low stock with multiple locations:** Sum `CountOnHand` per VariantId across all StockLocations, compare against per-location thresholds. A variant is "low stock" if ANY location falls below its threshold.

**Activity type mapping:** StockMovement entity may not have a human-readable `Type` string. Use `StockMovement.MovementType` or hardcode `"Stock"`. If the entity has no title field, construct: `"Stock: {variant.Sku ?? variantName}"`.

**Taxonomy/Taxon counts:** If Taxonomy/Taxon entities live in a different namespace (`Catalog.Domain.Taxonomies`), use `dbContext.Set<Taxonomy>()` which works cross-namespace within the same DbContext.

## 10. Validation Criteria

- `dotnet build` passes with warnings-as-errors
- `dotnet test --filter "Dashboard"` all pass
- Response JSON matches frontend `report.types.ts` interfaces (shape validation)
- Swagger/Scalar docs show `GET /api/dashboard` with correct response schema
- Manager role gets `200`, anonymous gets `401`, restricted role gets `403`
- No `Domain/` folder created under `Module/Dashboard/`
- No cross-module `using` directives (e.g., Dashboard handler should not import `Module.Ordering.Domain.Orders.Order` directly if using `Set<>()`)

## 11. Related Specifications / Further Reading

- [spec-design-admin-api-services.md](spec-design-admin-api-services.md) — Admin SPA API service layer mappings
- [spec-design-feature-conventions-remediation.md](spec-design-feature-conventions-remediation.md) — Vertical slice file conventions
- `docs/codebase/ARCHITECTURE.md` — Module isolation rules, dependency direction
- `docs/codebase/CONVENTIONS.md` — C# coding conventions, result object pattern
- `.harness/enforcement.yml` — Feature file naming, file limits
- `.harness/principles.yml` — Result objects, module isolation, CQRS handler discipline
