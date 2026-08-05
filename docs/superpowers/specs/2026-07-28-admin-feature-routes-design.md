# Admin Feature Routes & Views — Scaffold

**Date**: 2026-07-28
**Status**: Approved
**Context**: Admin SPA feature folder scaffolding needs routes and placeholder views for all 11 feature modules, mirroring backend API aggregates.

---

## Architecture

### Co-located feature routes
Each feature's `routes/index.ts` owns its route tree and menu items. The main router collects all feature route arrays and spreads them as children of `AdminLayout`.

```
features/{module}/
├── routes/index.ts    ← exports `{module}Routes` + `{module}MenuItems`
└── views/             ← lazy-loaded page-view components
```

### Main router integration
`app/router/routes.ts` imports all feature route arrays. The `AdminLayout` children spread each array:

```ts
import { catalogRoutes, catalogMenuItems } from '@/features/catalog/routes'
// ...

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: AdminLayout,
    children: [
      ...dashboardRoutes,
      ...catalogRoutes,
      ...identityRoutes,
      // ...
    ],
  },
  // auth, 404 standalone
]
```

Menu items are aggregated separately via a menu composable or import in `AppSidebar`.

---

## Page Patterns

| Pattern | Description | Route shape |
|---------|-------------|-------------|
| **List page** | Table with filters, CRUD via dialogs/drawers | `/{module}/{entity}` |
| **Detail page** | Tabs for sub-resources | `/{module}/{entity}/:id` |
| **Singleton page** | Single-page module (Dashboard, Permissions) | `/{module}` |
| **Auth page** | Standalone outside AdminLayout | `/auth/login` |

### CRUD handling
Create, edit, and delete operations happen within list or detail pages via PrimeVue dialogs/drawers — not separate routes. Detail pages use tabs for sub-resources (e.g., ProductDetail has "Variants", "Classifications", "Option Types" tabs).

---

## Route Tree

### Dashboard — 1 page
| Route | View | Notes |
|-------|------|-------|
| `/dashboard` | DashboardPage | Singleton dashboard |

### Catalog — 3 aggregates (7 pages)
| Route | View | Notes |
|-------|------|-------|
| `/catalog` | _redirect_ | → `/catalog/products` |
| `/catalog/products` | ProductsList | CRUD via dialogs |
| `/catalog/products/:id` | ProductDetail | Tabs: Details, Variants, Classifications, Option Types |
| `/catalog/taxonomies` | TaxonomiesList | |
| `/catalog/taxonomies/:id` | TaxonomyDetail | Tabs: Taxons, Rules |
| `/catalog/option-types` | OptionTypesList | |
| `/catalog/option-types/:id` | OptionTypeDetail | Tabs: Values |

### Identity — 3 aggregates (7 pages)
| Route | View | Notes |
|-------|------|-------|
| `/identity` | _redirect_ | → `/identity/users` |
| `/identity/users` | UsersList | |
| `/identity/users/:id` | UserDetail | Tabs: Roles, Permissions |
| `/identity/roles` | RolesList | |
| `/identity/roles/:id` | RoleDetail | Tabs: Permissions |
| `/identity/permissions` | PermissionsList | Read-only system permissions |

### Inventory — 5 aggregates (9 pages)
| Route | View | Notes |
|-------|------|-------|
| `/inventory` | _redirect_ | → `/inventory/stock-items` |
| `/inventory/stock-items` | StockItemsList | |
| `/inventory/stock-items/:id` | StockItemDetail | |
| `/inventory/stock-locations` | StockLocationsList | |
| `/inventory/stock-locations/:id` | StockLocationDetail | |
| `/inventory/stock-reservations` | StockReservationsList | Read-only; cancel row action |
| `/inventory/stock-transfers` | StockTransfersList | |
| `/inventory/stock-transfers/:id` | StockTransferDetail | Tabs: Items |
| `/inventory/stock-movements` | StockMovementsList | Read-only audit trail |

### Location — 2 aggregates (5 pages)
| Route | View | Notes |
|-------|------|-------|
| `/location` | _redirect_ | → `/location/countries` |
| `/location/countries` | CountriesList | |
| `/location/countries/:id` | CountryDetail | |
| `/location/states` | StatesList | |
| `/location/states/:id` | StateDetail | |

### Ordering — 1 aggregate (2 pages)
| Route | View | Notes |
|-------|------|-------|
| `/ordering` | _redirect_ | → `/ordering/orders` |
| `/ordering/orders` | OrdersList | |
| `/ordering/orders/:id` | OrderDetail | Tabs: Line Items |

### Payment — 2 aggregates (3 pages)
| Route | View | Notes |
|-------|------|-------|
| `/payment` | _redirect_ | → `/payment/payments` |
| `/payment/payments` | PaymentsList | Read-only; capture/void/refund row actions |
| `/payment/payment-methods` | PaymentMethodsList | |
| `/payment/payment-methods/:id` | PaymentMethodDetail | |

### Profile — 2 aggregates (5 pages)
| Route | View | Notes |
|-------|------|-------|
| `/profile` | _redirect_ | → `/profile/profiles` |
| `/profile/profiles` | ProfilesList | |
| `/profile/profiles/:id` | ProfileDetail | |
| `/profile/addresses` | AddressesList | |
| `/profile/addresses/:id` | AddressDetail | |

### Shipping — 2 aggregates (5 pages)
| Route | View | Notes |
|-------|------|-------|
| `/shipping` | _redirect_ | → `/shipping/shipping-methods` |
| `/shipping/shipping-methods` | ShippingMethodsList | |
| `/shipping/shipping-methods/:id` | ShippingMethodDetail | |
| `/shipping/shipping-rates` | ShippingRatesList | |
| `/shipping/shipping-rates/:id` | ShippingRateDetail | |

### Auth — 1 page (standalone)
| Route | View | Notes |
|-------|------|-------|
| `/auth/login` | LoginPage | Outside AdminLayout |

### 404 — catch-all
| Route | | Notes |
|-------|---|-------|
| `/:pathMatch(.*)*` | | Existing ErrorLayout |

---

## Menu Items

Each feature routes module exports a `MenuItem[]` array. Aggregated in sidebar. Structure:

| Label | Icon | Children | Route name |
|-------|------|----------|------------|
| Dashboard | `pi pi-chart-bar` | — | `dashboard` |
| Catalog | `pi pi-box` | Products, Taxonomies, Option Types | — |
| Identity | `pi pi-users` | Users, Roles, Permissions | — |
| Inventory | `pi pi-warehouse` | Items, Locations, Reservations, Transfers, Movements | — |
| Location | `pi pi-map-marker` | Countries, States | — |
| Ordering | `pi pi-shopping-cart` | Orders | — |
| Payment | `pi pi-credit-card` | Payments, Methods | — |
| Profile | `pi pi-id-card` | Profiles, Addresses | — |
| Shipping | `pi pi-truck` | Methods, Rates | — |

---

## Auth Guard

Temporarily disabled for review. The `beforeEach` guard in `app/router/guards.ts` is commented out with:
```ts
// TODO: re-enable auth guard after route scaffold review
```

---

## Placeholder View Template

Each scaffolded view is a minimal Vue SFC:

```vue
<script setup lang="ts">
import PageShell from '@/shared/components/ui/PageShell.vue'
import PageHeading from '@/shared/components/ui/PageHeading.vue'
</script>

<template>
  <PageShell>
    <PageHeading title="Dashboard" />
    <p class="text-muted-color">Dashboard content coming soon.</p>
  </PageShell>
</template>
```

---

## View Files List (38 total)

### auth/
- LoginPage.vue

### dashboard/
- DashboardPage.vue

### catalog/views/
- ProductsList.vue, ProductDetail.vue
- TaxonomiesList.vue, TaxonomyDetail.vue
- OptionTypesList.vue, OptionTypeDetail.vue

### identity/views/
- UsersList.vue, UserDetail.vue
- RolesList.vue, RoleDetail.vue
- PermissionsList.vue

### inventory/views/
- StockItemsList.vue, StockItemDetail.vue
- StockLocationsList.vue, StockLocationDetail.vue
- StockReservationsList.vue
- StockTransfersList.vue, StockTransferDetail.vue
- StockMovementsList.vue

### location/views/
- CountriesList.vue, CountryDetail.vue
- StatesList.vue, StateDetail.vue

### ordering/views/
- OrdersList.vue, OrderDetail.vue

### payment/views/
- PaymentsList.vue
- PaymentMethodsList.vue, PaymentMethodDetail.vue

### profile/views/
- ProfilesList.vue, ProfileDetail.vue
- AddressesList.vue, AddressDetail.vue

### shipping/views/
- ShippingMethodsList.vue, ShippingMethodDetail.vue
- ShippingRatesList.vue, ShippingRateDetail.vue
