# Per-Module Dashboards

**Date**: 2025-07-17
**Status**: designed, not implemented

## Overview

Add per-module dashboard endpoints for Catalog, Ordering, and Inventory so each
module owner gets domain-specific KPIs beyond what the global dashboard provides.
The global dashboard (`GET /api/dashboard`) stays unchanged as the cross-module
pulse check.

## Architecture

Each per-module dashboard lives **inside its own module** (not in the central
`Dashboard` module), following the existing vertical-slice pattern. A module's
dashboard handler queries only that module's entities.

```
Module/
├── Catalog/Features/Admin/Dashboard/Get/
│   ├── GetCatalogDashboard.cs
│   ├── GetCatalogDashboard.Request.cs
│   ├── GetCatalogDashboard.Response.cs
│   └── GetCatalogDashboard.Endpoint.cs
├── Ordering/Features/Admin/Dashboard/Get/    (same pattern)
├── Inventory/Features/Admin/Dashboard/Get/   (same pattern)
└── Dashboard/                                (existing, unchanged)
```

**URL scheme:**

| Endpoint | Module |
|----------|--------|
| `GET /api/dashboard` | Dashboard (cross-module, exists) |
| `GET /api/catalog/dashboard` | Catalog (new) |
| `GET /api/ordering/dashboard` | Ordering (new) |
| `GET /api/inventory/dashboard` | Inventory (new) |

## Catalog Dashboard

### Endpoint

`GET /api/catalog/dashboard`

### Response

```csharp
record CatalogDashboardResponse
{
    int TotalProducts;
    int ActiveProducts;
    int DraftProducts;
    int TotalVariants;
    int TotalTaxonomies;
    int TotalTaxons;
    int DigitalAssetCount;
    List<RecentProduct> RecentProducts;
}

record RecentProduct(string Id, string Name, string Sku, DateTime CreatedAt);
```

### Behavior

- `ActiveProducts` = products with status `Active` and not soft-deleted.
- `DraftProducts` = products with status `Draft` and not soft-deleted.
- `RecentProducts` = top 5 products ordered by `CreatedAt` descending.

## Ordering Dashboard

### Endpoint

`GET /api/ordering/dashboard`

### Response

```csharp
record OrderingDashboardResponse
{
    int TotalOrders;
    int PendingFulfillment;
    int TodayOrders;
    decimal AverageOrderValue;
    decimal TotalRevenue;
    List<RecentOrder> RecentOrders;
    OrderStatusBreakdown StatusBreakdown;
}

record RecentOrder(string Id, string OrderNumber, string CustomerName, decimal Total, string Status, DateTime CreatedAt);
record OrderStatusBreakdown(int Draft, int Placed, int Canceled, int Expired);
```

### Behavior

- `PendingFulfillment` = orders with status `Placed` (ready for fulfillment).
- `TotalOrders`, `TotalRevenue`, `AverageOrderValue` and status breakdown exclude
  soft-deleted orders (consistent with global dashboard).
- `TodayOrders` = orders where `CreatedAt` is today (UTC).
- `AverageOrderValue` = `TotalRevenue / TotalOrders` (0 if no orders).
- `RecentOrders` = top 10 by `CreatedAt` descending.
- `StatusBreakdown` covers all four `OrderStatus` values: Draft, Placed, Canceled, Expired.

## Inventory Dashboard

### Endpoint

`GET /api/inventory/dashboard`

### Response

```csharp
record InventoryDashboardResponse
{
    int TotalSkusTracked;
    int InStockCount;
    int OutOfStockCount;
    int LowStockCount;
    int StockLocationCount;
    int ItemsPerLocationAverage;
    List<RecentStockMovement> RecentMovements;
}

record RecentStockMovement(string Id, string Sku, string Location, int QuantityChange, string Type, DateTime OccurredAt);
```

### Behavior

- `LowStockCount` = stock items where `CountOnHand > 0` and `<= StockLocation.LowStockThreshold`
  (same logic as global dashboard).
- `RecentMovements` = top 10 by `OccurredAt` descending.

## Permissions

Three new permissions registered in `Shared/Security/Authorization/Features/DashboardFeatureMetadata.cs`,
following the existing pattern:

| Permission | Guards |
|------------|--------|
| `Dashboard.Catalog.List` | `GET /api/catalog/dashboard` |
| `Dashboard.Orders.List` | `GET /api/ordering/dashboard` |
| `Dashboard.Inventory.List` | `GET /api/inventory/dashboard` |

`PermissionContext` must be updated to include these three new resources.

## Admin SPA Changes

### Catalog

- Replace stub store `fetchSummary()` in `features/catalog/dashboard/stores/catalog-dashboard.store.ts`
  with a live call to `GET /api/catalog/dashboard`.
- Add `catalog-dashboard.service.ts` with Axios call.
- The view (`CatalogDashboard.vue`) already exists and renders; just needs real data.

### Ordering

New directory `features/ordering/dashboard/`:

- `views/OrderingDashboard.vue` — stat cards + recent orders table + status breakdown chart.
- `stores/ordering-dashboard.store.ts` — Pinia store with `fetchDashboard()`.
- `services/ordering-dashboard.service.ts` — Axios call to `GET /api/ordering/dashboard`.
- `types/ordering-dashboard.types.ts` — TypeScript interfaces.

Route `/ordering` → ordering dashboard (new root route for the ordering feature).

### Inventory

New directory `features/inventory/dashboard/`:

- `views/InventoryDashboard.vue` — stat cards + recent movements table.
- `stores/inventory-dashboard.store.ts` — Pinia store with `fetchDashboard()`.
- `services/inventory-dashboard.service.ts` — Axios call to `GET /api/inventory/dashboard`.
- `types/inventory-dashboard.types.ts` — TypeScript interfaces.

Route `/inventory` → inventory dashboard (new root route for the inventory feature).

All follow the same Pinia store + Axios service + Vue view pattern as the existing
reports module (`features/reports/`).

## Testing

### Backend

Three new test files under `tests/Module.UnitTests/`:

- `Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboardHandlerTests.cs`
- `Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboardHandlerTests.cs`
- `Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboardHandlerTests.cs`

Each mirrors the existing 6-test-case pattern in `Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs`:
empty DB, normal data, filtered counts (exclude deleted/draft/canceled), and edge cases.

### Frontend

Store unit tests per dashboard store verifying correct state mapping from API response.

## Out of Scope

- Payment, Identity, Location, Profile, Shipping modules — no dashboards. These are
  management UIs, not analytics dashboards.
- Real-time updates (polling, SignalR) — dashboards are snapshot-on-load.
- Date-range filtering — follow-up feature, not in this iteration.
