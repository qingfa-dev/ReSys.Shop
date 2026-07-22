---
title: Admin SPA — Consistent List + Detail Page Pattern
version: 2.0
date_created: 2026-07-22
last_updated: 2026-07-23
owner: Admin SPA team
tags: [design, app, admin-spa, vue, primevue]
---

# Introduction

Replace all admin SPA stub pages with real list + detail pages following a
single consistent pattern across all modules. Drop specialized page variants
(`TreeManagerPage`, separate create pages, separate sub-entity list pages) so
every top-level entity follows the same `ListPage` + `DetailPage` convention.
Align all feature module folder structures to the `auth/` module pattern.

## 1. Purpose & Scope

**Purpose:** Define the page model, routing pattern, component tree, sub-entity
management, folder structure, and consistency rules for all admin SPA pages.

**Scope:** All admin modules — Catalog, Inventory, Ordering, Payment, Shipping,
Location, Users, Profile, Reports. Every existing stub page is either replaced
or removed. Auth module is out of scope (already implemented).

**Assumptions:**
- PrimeVue v5, Tailwind v4, and the existing shared component suite are used
- Backend API endpoints exist for all entities (verified: 180 admin endpoints)
- Existing `src/shared/api/` Axios infrastructure is reused
- No new npm dependencies

## 2. Definitions

- **ListPage** — A page rendering a `DataTable` with search, filter, pagination, row actions
- **DetailPage** — A single page handling create, view, and edit modes via route-driven mode detection
- **Top-level entity** — A domain entity that has its own route and API CRUD endpoints
- **Sub-entity** — An entity owned by a parent, managed inline on the parent's DetailPage
- **MPTT** — Modified Preorder Tree Traversal (the nested-set hierarchy used by Taxons)
- **Feature module** — A directory under `src/features/<module>/` with a standard subdirectory layout

## 3. Feature Module Folder Structure

Every module follows the `auth/` pattern. Each module directory contains:

```
src/features/<module>/
├── api/            # Module-specific API service functions
├── components/     # Feature-scoped reusable components
├── composables/    # Shared reactive logic
├── models/         # TypeScript interfaces/types for API responses and forms
├── pages/          # Page components (ListPage, DetailPage, DashboardPage)
├── store/          # Pinia stores (optional, per need)
├── utils/          # Validation, formatting helpers
└── routes.ts       # Route definitions (exported RouteRecordRaw)
```

**Migration:** Existing modules with flat `pages/` directories (catalog, inventory, ordering,
payment, shipping, location, users, profile, reports) must be restructured to this layout.
Move existing stub pages into `pages/`, create empty `api/`, `components/`, `composables/`,
`models/`, `store/`, `utils/` directories.

## 4. Requirements, Constraints & Guidelines

- **PAT-001**: Every top-level entity gets exactly 1 `ListPage` and 1 `DetailPage`
- **PAT-002**: Sub-entities are rendered inline on their parent's `DetailPage` as `<Fieldset>` sections. No separate routes.
- **PAT-003**: Route pattern per entity: `/module/entity` (list), `/module/entity/new` (create), `/module/entity/:id` (view), `/module/entity/:id/edit` (edit). 4 routes, 2 components.
- **PAT-004**: No specialized page types (TreeManager, separate Create pages, separate View pages)
- **PAT-005**: Pages use shared components directly. No generic composable layer — consistency via convention.
- **PAT-006**: Taxon hierarchy shown as flat DataTable with CSS depth indentation, not a Tree component
- **GUD-001**: Use existing API infrastructure: Axios `apiClient` (`src/shared/api/client.ts`), `createModuleApi` factory (`src/shared/api/services/module-api.factory.ts`), `Result<T>` / `PagedResult<T>` types (`src/shared/models/`), `resultToMapped` / `pagedResultToMapped` mappers. Module-specific API calls go in `<module>/api/`.
- **GUD-002**: Client-side form validation only, inline per page. No validation library.
- **CON-001**: Use existing `useToastNotify` for feedback, `useConfirm` for destructive actions
- **CON-002**: No new npm packages or dependencies
- **CON-003**: Route `:id` param is a GUID string — API path-building uses `/{id}` to match backend `{id:guid}` constraints

## 5. Entity-to-Page Mapping

**Key:** L = ListPage, D = DetailPage, K = Keep existing, — = removed

### Catalog (7 existing → 7 pages, 4 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| Product | `ProductListPage.vue` (K) | `ProductDetailPage.vue` (new) | `ProductCreatePage.vue` |
| Taxonomy | `TaxonomyListPage.vue` (new) | `TaxonomyDetailPage.vue` (new) | `TaxonTreeManagerPage.vue`, `TaxonListPage.vue` |
| OptionType | `OptionTypeListPage.vue` (K) | `OptionTypeDetailPage.vue` (new) | `OptionValueListPage.vue` |

**Sub-entities inline:** Variants, Prices, Images, Classifications on ProductDetailPage.
Taxons on TaxonomyDetailPage. OptionValues on OptionTypeDetailPage.

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| Product | `GET /api/catalog/products` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| Taxonomy | `GET /api/catalog/taxonomies` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| OptionType | `GET /api/catalog/option-types` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| Taxons (sub) | `GET .../{taxonomyId}/taxons` | — | — | — | — |
| Variants (sub) | `GET .../{productId}/variants` | — | — | — | — |

### Inventory (7 existing → 9 pages, 2 removed, 4 new)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| StockItem | `StockListPage.vue` (K) | `StockItemDetailPage.vue` (new) | `StockImportPage.vue` |
| StockLocation | `LocationListPage.vue` (K) | `LocationDetailPage.vue` (new) | — |
| StockMovement | `MovementListPage.vue` (K) | — (read-only log) | — |
| StockTransfer | `TransferListPage.vue` (K) | `TransferDetailPage.vue` (new) | — |
| StockReservation | `StockReservationListPage.vue` (new) | — (view + cancel from list) | — |
| — | — | — | `UnitListPage.vue` (no backend entity) |

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| StockItem | `GET /api/inventory/stock-items` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| StockLocation | `GET .../stock-locations` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| StockMovement | `GET .../stock-movements` | `GET .../{id}` | — | — | — |
| StockTransfer | `GET .../stock-transfers` | `GET .../{id}` | `POST ...` | — | — |
| StockReservation | `GET .../stock-reservations` | — | — | — | `POST .../{id}/cancel` |

### Ordering (4 existing → 4 pages, 1 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| Order | `OrderListPage.vue` (K) | `OrderDetailPage.vue` (new) | `OrderCreatePage.vue` |
| Fulfillment | `FulfillmentQueuePage.vue` (K) | — (read-only) | — |

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| Order | `GET /api/ordering/orders` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| Fulfillment | (via order status transitions) | — | — | — | — |

### Payment (2 existing → 4 pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| Payment | `PaymentListPage.vue` (K) | `PaymentDetailPage.vue` (new) |
| PaymentMethod | `PaymentMethodListPage.vue` (K) | `PaymentMethodDetailPage.vue` (new) |

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| Payment | `GET /api/payment/payments` | `GET .../{id}` | — | — | — |
| PaymentMethod | `GET .../payment-methods` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |

### Shipping (2 existing → 4 pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| ShippingMethod | `ShippingMethodListPage.vue` (K) | `ShippingMethodDetailPage.vue` (new) |
| ShippingRate | `ShippingRateListPage.vue` (K) | `ShippingRateDetailPage.vue` (new) |

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| ShippingMethod | `GET /api/shipping/shipping-methods` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| ShippingRate | `GET .../shipping-rates` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |

### Location (2 existing → 4 pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| Country | `CountryListPage.vue` (K) | `CountryDetailPage.vue` (new) |
| State | `StateListPage.vue` (K) | `StateDetailPage.vue` (new) |

**API backing:**
| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| Country | `GET /api/locations/countries` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| State | `GET .../states` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |

### Users (5 existing → 8 pages, 1 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Staff | `StaffListPage.vue` (K) | `StaffDetailPage.vue` (new) | `StaffCreatePage.vue` |
| Customer | `CustomerListPage.vue` (K) | `CustomerDetailPage.vue` (new) | — |
| Role | `RoleListPage.vue` (K) | `RoleDetailPage.vue` (new) | — |
| Permission | `PermissionListPage.vue` (K) | `PermissionDetailPage.vue` (new) | — |

**API backing:** Staff and Customer share the Identity Users API — filter by role on the list.
StaffDetailPage manages user roles/permissions via sub-tables.

| Entity | List | GetById | Create | Update | Delete |
|--------|------|---------|--------|--------|--------|
| Users (Staff+Customer) | `GET /api/identity/users` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| Roles | `GET /api/identity/roles` | `GET .../{id}` | `POST ...` | `PUT .../{id}` | `DELETE .../{id}` |
| Permissions | `GET /api/identity/permissions` | — (all returned) | — | — | — |

### Profile (2 existing → 2 pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| UserProfile | — | `ProfilePage.vue` (K) |
| Address | `AddressListPage.vue` (K) | — (sub-entity of profile) |

### Reports (1 existing → 1 page, 0 removed)

| Entity | Page |
|--------|------|
| Dashboard | `DashboardPage.vue` (K) |

### Summary

| Metric | Count |
|--------|-------|
| Existing pages (keep) | 24 |
| New page files | 19 |
| Pages to remove | 8 |
| **Total resulting pages** | **43** |

**New page files to create (19 — 17 detail + 2 list):**

```
app/Admin/src/features/catalog/pages/ProductDetailPage.vue
app/Admin/src/features/catalog/pages/TaxonomyListPage.vue
app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue
app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue
app/Admin/src/features/inventory/pages/StockItemDetailPage.vue
app/Admin/src/features/inventory/pages/LocationDetailPage.vue
app/Admin/src/features/inventory/pages/TransferDetailPage.vue
app/Admin/src/features/inventory/pages/StockReservationListPage.vue
app/Admin/src/features/ordering/pages/OrderDetailPage.vue
app/Admin/src/features/payment/pages/PaymentDetailPage.vue
app/Admin/src/features/payment/pages/PaymentMethodDetailPage.vue
app/Admin/src/features/shipping/pages/ShippingMethodDetailPage.vue
app/Admin/src/features/shipping/pages/ShippingRateDetailPage.vue
app/Admin/src/features/location/pages/CountryDetailPage.vue
app/Admin/src/features/location/pages/StateDetailPage.vue
app/Admin/src/features/users/pages/StaffDetailPage.vue
app/Admin/src/features/users/pages/CustomerDetailPage.vue
app/Admin/src/features/users/pages/RoleDetailPage.vue
app/Admin/src/features/users/pages/PermissionDetailPage.vue
```

**Pages to remove (8):**

```
app/Admin/src/features/catalog/pages/ProductCreatePage.vue
app/Admin/src/features/catalog/pages/TaxonListPage.vue
app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue
app/Admin/src/features/catalog/pages/OptionValueListPage.vue
app/Admin/src/features/inventory/pages/StockImportPage.vue
app/Admin/src/features/inventory/pages/UnitListPage.vue
app/Admin/src/features/ordering/pages/OrderCreatePage.vue
app/Admin/src/features/users/pages/StaffCreatePage.vue
```

## 6. Routing

### Pattern per entity

```
/module/entity              → XxxListPage.vue
/module/entity/new          → XxxDetailPage.vue  (mode: create)
/module/entity/:id          → XxxDetailPage.vue  (mode: view)
/module/entity/:id/edit     → XxxDetailPage.vue  (mode: edit)
```

4 routes, 2 page components per entity. `:id` is a GUID string; API call paths
substitute `/{id}` to match backend `{id:guid}` constraints.

### Mode detection (in every DetailPage)

```ts
const route = useRoute()
const id = computed(() => route.params.id as string | undefined)
const mode = computed(() =>
  !id.value ? 'create'
    : route.name?.toString().endsWith('.edit') ? 'edit'
    : 'view'
)
```

### Complete route definitions per module

#### Catalog routes (`catalog/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `catalog` | redirect → `catalog.dashboard` | — |
| `catalog/dashboard` | `catalog.dashboard` | `DashboardPage.vue` |
| `catalog/products` | `catalog.products.list` | `ProductListPage.vue` |
| `catalog/products/new` | `catalog.products.create` | `ProductDetailPage.vue` |
| `catalog/products/:id` | `catalog.products.view` | `ProductDetailPage.vue` |
| `catalog/products/:id/edit` | `catalog.products.edit` | `ProductDetailPage.vue` |
| `catalog/taxonomies` | `catalog.taxonomies.list` | `TaxonomyListPage.vue` |
| `catalog/taxonomies/new` | `catalog.taxonomies.create` | `TaxonomyDetailPage.vue` |
| `catalog/taxonomies/:id` | `catalog.taxonomies.view` | `TaxonomyDetailPage.vue` |
| `catalog/taxonomies/:id/edit` | `catalog.taxonomies.edit` | `TaxonomyDetailPage.vue` |
| `catalog/option-types` | `catalog.option-types.list` | `OptionTypeListPage.vue` |
| `catalog/option-types/new` | `catalog.option-types.create` | `OptionTypeDetailPage.vue` |
| `catalog/option-types/:id` | `catalog.option-types.view` | `OptionTypeDetailPage.vue` |
| `catalog/option-types/:id/edit` | `catalog.option-types.edit` | `OptionTypeDetailPage.vue` |

Removed: `catalog/products/create` (→ `products/new`), `catalog/taxa` (replaced by taxonomies sub-entity), `catalog/option-values` (→ option-types sub-entity).

#### Inventory routes (`inventory/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `inventory` | redirect → `inventory.dashboard` | — |
| `inventory/dashboard` | `inventory.dashboard` | `DashboardPage.vue` |
| `inventory/stocks` | `inventory.stocks.list` | `StockListPage.vue` |
| `inventory/stocks/new` | `inventory.stocks.create` | `StockItemDetailPage.vue` |
| `inventory/stocks/:id` | `inventory.stocks.view` | `StockItemDetailPage.vue` |
| `inventory/stocks/:id/edit` | `inventory.stocks.edit` | `StockItemDetailPage.vue` |
| `inventory/locations` | `inventory.locations.list` | `LocationListPage.vue` |
| `inventory/locations/new` | `inventory.locations.create` | `LocationDetailPage.vue` |
| `inventory/locations/:id` | `inventory.locations.view` | `LocationDetailPage.vue` |
| `inventory/locations/:id/edit` | `inventory.locations.edit` | `LocationDetailPage.vue` |
| `inventory/movements` | `inventory.movements.list` | `MovementListPage.vue` |
| `inventory/transfers` | `inventory.transfers.list` | `TransferListPage.vue` |
| `inventory/transfers/new` | `inventory.transfers.create` | `TransferDetailPage.vue` |
| `inventory/transfers/:id` | `inventory.transfers.view` | `TransferDetailPage.vue` |
| `inventory/transfers/:id/edit` | `inventory.transfers.edit` | `TransferDetailPage.vue` |
| `inventory/reservations` | `inventory.reservations.list` | `StockReservationListPage.vue` |

Removed: `inventory/stocks/import` (→ action on StockListPage), `inventory/units` (no backend entity).

#### Ordering routes (`ordering/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `ordering` | redirect → `ordering.dashboard` | — |
| `ordering/dashboard` | `ordering.dashboard` | `DashboardPage.vue` |
| `ordering/orders` | `ordering.orders.list` | `OrderListPage.vue` |
| `ordering/orders/new` | `ordering.orders.create` | `OrderDetailPage.vue` |
| `ordering/orders/:id` | `ordering.orders.view` | `OrderDetailPage.vue` |
| `ordering/orders/:id/edit` | `ordering.orders.edit` | `OrderDetailPage.vue` |
| `ordering/fulfillment` | `ordering.fulfillment.queue` | `FulfillmentQueuePage.vue` |

Removed: `ordering/orders/create` (→ `orders/new`).

#### Payment routes (`payment/routes.ts`)

Normalize path: drop legacy `/payments/list` for `/payments`.

| Path | Name | Component |
|------|------|-----------|
| `payments` | redirect → `payment.payments.list` | — |
| `payments/payments` | `payment.payments.list` | `PaymentListPage.vue` |
| `payments/payments/new` | `payment.payments.create` | `PaymentDetailPage.vue` |
| `payments/payments/:id` | `payment.payments.view` | `PaymentDetailPage.vue` |
| `payments/payments/:id/edit` | `payment.payments.edit` | `PaymentDetailPage.vue` |
| `payments/methods` | `payment.methods.list` | `PaymentMethodListPage.vue` |
| `payments/methods/new` | `payment.methods.create` | `PaymentMethodDetailPage.vue` |
| `payments/methods/:id` | `payment.methods.view` | `PaymentMethodDetailPage.vue` |
| `payments/methods/:id/edit` | `payment.methods.edit` | `PaymentMethodDetailPage.vue` |

Changed: `/payments/list` → `/payments/payments` to follow `/module/entity` plural convention.

#### Shipping routes (`shipping/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `shipping` | redirect → `shipping.methods.list` | — |
| `shipping/methods` | `shipping.methods.list` | `ShippingMethodListPage.vue` |
| `shipping/methods/new` | `shipping.methods.create` | `ShippingMethodDetailPage.vue` |
| `shipping/methods/:id` | `shipping.methods.view` | `ShippingMethodDetailPage.vue` |
| `shipping/methods/:id/edit` | `shipping.methods.edit` | `ShippingMethodDetailPage.vue` |
| `shipping/rates` | `shipping.rates.list` | `ShippingRateListPage.vue` |
| `shipping/rates/new` | `shipping.rates.create` | `ShippingRateDetailPage.vue` |
| `shipping/rates/:id` | `shipping.rates.view` | `ShippingRateDetailPage.vue` |
| `shipping/rates/:id/edit` | `shipping.rates.edit` | `ShippingRateDetailPage.vue` |

#### Location routes (`location/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `locations` | redirect → `location.countries.list` | — |
| `locations/countries` | `location.countries.list` | `CountryListPage.vue` |
| `locations/countries/new` | `location.countries.create` | `CountryDetailPage.vue` |
| `locations/countries/:id` | `location.countries.view` | `CountryDetailPage.vue` |
| `locations/countries/:id/edit` | `location.countries.edit` | `CountryDetailPage.vue` |
| `locations/states` | `location.states.list` | `StateListPage.vue` |
| `locations/states/new` | `location.states.create` | `StateDetailPage.vue` |
| `locations/states/:id` | `location.states.view` | `StateDetailPage.vue` |
| `locations/states/:id/edit` | `location.states.edit` | `StateDetailPage.vue` |

#### Users routes (`users/routes.ts`)

| Path | Name | Component |
|------|------|-----------|
| `users` | redirect → `users.staff.list` | — |
| `users/staff` | `users.staff.list` | `StaffListPage.vue` |
| `users/staff/new` | `users.staff.create` | `StaffDetailPage.vue` |
| `users/staff/:id` | `users.staff.view` | `StaffDetailPage.vue` |
| `users/staff/:id/edit` | `users.staff.edit` | `StaffDetailPage.vue` |
| `users/customers` | `users.customers.list` | `CustomerListPage.vue` |
| `users/customers/new` | `users.customers.create` | `CustomerDetailPage.vue` |
| `users/customers/:id` | `users.customers.view` | `CustomerDetailPage.vue` |
| `users/customers/:id/edit` | `users.customers.edit` | `CustomerDetailPage.vue` |
| `users/roles` | `users.roles.list` | `RoleListPage.vue` |
| `users/roles/new` | `users.roles.create` | `RoleDetailPage.vue` |
| `users/roles/:id` | `users.roles.view` | `RoleDetailPage.vue` |
| `users/roles/:id/edit` | `users.roles.edit` | `RoleDetailPage.vue` |
| `users/permissions` | `users.permissions.list` | `PermissionListPage.vue` |
| `users/permissions/new` | `users.permissions.create` | `PermissionDetailPage.vue` |
| `users/permissions/:id` | `users.permissions.view` | `PermissionDetailPage.vue` |
| `users/permissions/:id/edit` | `users.permissions.edit` | `PermissionDetailPage.vue` |

Removed: `users/staff/create` (→ `staff/new`).

#### Profile routes (`profile/routes.ts`)

Normalize route names to `profile.*` namespace:

| Path | Name | Component |
|------|------|-----------|
| `profile` | `profile.view` | `ProfilePage.vue` |
| `profile/addresses` | `profile.addresses` | `AddressListPage.vue` |

Changed: `profile` → `profile.view`, `addresses` → `profile.addresses` for naming consistency.

#### Reports routes (`reports/routes.ts`)

No changes.

## 7. Component Tree

### ListPage

```
PageHeader (breadcrumb, title, subtitle, actions slot)
TableToolbar (search input, filter buttons, "+ Create" button)
DataTable (lazy load, sort, pagination, striped rows)
  ├── EmptyState (no data)
  ├── LoadingSkeleton (fetching)
  ├── ErrorState (fetch failed, retry button)
  ├── row-click → navigate to :id (view mode)
  └── ActionMenu per row (edit, delete)
BulkActionBar (floating, visible when rows selected)
```

### DetailPage

```
PageHeader (breadcrumb, title, subtitle)
Card — entity fields
  └── FormField (label, required indicator, input slot, hint, error) × N
├── Fieldset "Sub-entity A" (if applicable)
│   ├── TableToolbar (search, "+ Add")
│   ├── DataTable (sub-entity rows)
│   └── EmptyState / LoadingSkeleton / ErrorState
├── Fieldset "Sub-entity B" ...
└── FormActions (sticky bottom: Save + Cancel)
```

### Sub-entity add/edit flow

"+ Add" or row "Edit" opens a slideover (PrimeVue `Drawer`) with the sub-entity
form. On save, the sub-table refreshes without navigation. Nested sub-entities
(Variant → Prices, Images) use tabs inside the slideover.

## 8. Taxon Depth Indentation

Taxons rendered as flat array in `TaxonomyDetailPage`'s taxons sub-table,
sorted by `lft` ASC. Visual indent = `depth × 1.5rem` with tree-line
`::before` pseudo on first column. No `Tree` or `TreeTable` component.

| depth | display       |
|-------|---------------|
| 0     | Clothing      |
| 1     | ├─ Men's      |
| 2     | ├── Tops      |
| 2     | ├── Bottoms    |
| 1     | ├─ Women's    |

## 9. Menu Changes

**Entries to remove (6):**

| Menu Entry | Route Name | Reason |
|-----------|-----------|--------|
| "Add Product" | `catalog.products.create` (old) | Deprecated route; creation via `products/new` |
| "All Categories" | `catalog.taxa.list` | TaxonListPage removed; taxons on TaxonomyDetailPage |
| "Values" | `catalog.option-values.list` | OptionValueListPage removed; values on OptionTypeDetailPage |
| "Import" | `inventory.stocks.import` | Becomes action button on StockListPage |
| "Stock Units" | `inventory.units.list` | No backend entity |
| "Invite Staff" | `users.staff.create` (old) | Deprecated route; creation via `staff/new` |

**Entries to rename (2):**

| Old | New Label | New Route | Reason |
|-----|-----------|-----------|--------|
| "Manager" | "All Taxonomies" | `catalog.taxonomies.list` | TreeManagerPage replaced by TaxonomyListPage |
| "Create Order" | "All Orders" sub-item removed? No — keep as "All Orders" with route `ordering.orders.list`. Actually this item was already in "All Orders" submenu. Just remove the "Create Order" child. See note. | | |

**Note:** "Create Order" was a child of "All Orders" menu group. Remove the child entry
and keep the "All Orders" parent linking to `ordering.orders.list`. Creation
done via "+ Create" button on OrderListPage. Same pattern for "Add Product" /
"Invite Staff".

## 10. API Layer

### Existing infrastructure (reuse, do not rebuild)

All API communication uses the existing Axios-based stack:

```
src/shared/api/
├── client.ts                          # Axios instance (JWT, camelCase, error wrap)
├── services/module-api.factory.ts      # createModuleApi() per-endpoint helper
├── utils/result.mapper.ts             # resultToMapped, pagedResultToMapped
├── interceptors/
│   ├── auth.interceptor.ts            # JWT Bearer token
│   ├── camelcase.interceptor.ts       # camelCase ↔ PascalCase
│   └── error-wrapper.interceptor.ts   # Error normalization
└── index.ts                           # Barrel export
```

### Per-module API services

Each module's `api/` directory contains typed service functions using `apiClient` + `moduleApiFactory` or direct Axios calls. Example pattern for Catalog:

```ts
// features/catalog/api/products.ts
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult } from '@/shared/models'
import type { ProductResponse, ProductRequest } from '../models/Product'

export async function getProducts(params): Promise<MappedResult<ProductResponse[]>> {
  const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params })
  return pagedResultToMapped(res.data)
}

export async function getProduct(id: string): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.get<Result<ProductResponse>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}

export async function createProduct(data: ProductRequest): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.post<Result<ProductResponse>>('/catalog/products', data)
  return resultToMapped(res.data)
}

export async function updateProduct(id: string, data: ProductRequest): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteProduct(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}
```

### Shared models (existing)

```
src/shared/models/
├── result.ts       # Result<T>, PagedResult<T>, ApiProblemDetail
├── pagination.ts   # PaginationMeta
├── querying.ts     # QueryingParameters
└── api.ts          # ApiError, etc.
```

Module-specific response/request types go in `<module>/models/`.

### Form validation

Client-side only, inline per page:

```ts
const errors = reactive<Record<string, string>>({})
function validate(data): boolean {
  errors.name = !data.name ? 'Required' : ''
  return !Object.values(errors).some(Boolean)
}
```

`FormField` accepts `error` prop — wired directly.

## 11. Testing

- **Shared component tests** (existing): `DataTable`, `FormField`, `TableToolbar`, `PageHeader`
- **Page smoke tests**: mount without crashing, render `PageHeader` with correct title, handle loading / empty / error states
- **API service tests**: mock `apiClient`, verify request shape and response mapping
- **No integration tests**: API calls mocked

## 12. Acceptance Criteria

- **AC-001**: Every top-level entity has a working `ListPage` with `DataTable`, search, pagination, create/edit/delete actions.
- **AC-002**: Every top-level entity's `DetailPage` opens in view mode at `/:id`, switches to edit mode at `/:id/edit`, and opens blank form in create mode at `/new`.
- **AC-003**: ProductDetailPage renders Variants, Prices, Images, Classifications as inline sub-tables within `<Fieldset>` sections.
- **AC-004**: TaxonomyDetailPage renders Taxons as flat indented table sorted by `lft`, no `Tree` component.
- **AC-005**: All 8 removed page files no longer exist: `ProductCreatePage`, `TaxonListPage`, `TaxonTreeManagerPage`, `OptionValueListPage`, `StockImportPage`, `UnitListPage`, `OrderCreatePage`, `StaffCreatePage`.
- **AC-006**: Each module directory matches the auth/ folder structure (api/, components/, composables/, models/, pages/, store/, utils/, routes.ts).
- **AC-007**: Destructive actions (delete entity) show confirmation dialog via `useConfirm`.
- **AC-008**: Successful save/delete shows toast notification via `useToastNotify`.
- **AC-009**: All 6 dropped + 2 renamed menu entries updated in `admin-menu.config.ts`.
- **AC-010**: Profile route names use `profile.*` namespace: `profile.view`, `profile.addresses`.
- **AC-011**: Payment routes normalized: `/payments/payments` not `/payments/list`.
- **AC-012**: All API calls use existing Axios `apiClient` and `resultToMapped` / `pagedResultToMapped`, no new `fetch` wrapper.

## 13. Rationale & Context

Pages follow convention over abstraction because a generic composable layer would
require escape hatches for complex entities (Product with 5 sub-entity tables).
Direct component usage keeps each page explicit while shared components enforce
visual consistency.

Sub-entities are inline rather than separate routes because navigating away from
a Product detail page to manage variants breaks workflow. Slideovers provide
context-preserving editing.

Taxon depth indentation uses a flat `DataTable` rather than `TreeTable` because
the MPTT `lft`/`rgt`/`depth` model maps naturally to a sorted flat array.
Tree components add complexity for operations like reordering and bulk actions
that are simpler in a flat table.

Folder structure aligned to `auth/` because it provides clear boundaries:
API code, components, composables, and types are co-located per module,
enabling independent understanding and testing of each feature.

## 14. Dependencies & External Integrations

- **PLT-001**: Vue 3.5 + TypeScript 6 — runtime platform
- **PLT-002**: PrimeVue 5 + Aura preset — component library
- **PLT-003**: Tailwind v4 — utility CSS
- **PLT-004**: Axios (existing `apiClient`) — HTTP client
- **SVC-001**: Catalog API — 62 admin endpoints (`/api/catalog/*`)
- **SVC-002**: Inventory API — 28 admin endpoints (`/api/inventory/*`)
- **SVC-003**: Ordering API — 19 admin endpoints (`/api/ordering/*`)
- **SVC-004**: Payment API — 12 admin endpoints (`/api/payment/*`)
- **SVC-005**: Shipping API — 12 admin endpoints (`/api/shipping/*`)
- **SVC-006**: Location API — 12 admin endpoints (`/api/locations/*`)
- **SVC-007**: Identity API — 24 admin endpoints (`/api/identity/*`)
- **SVC-008**: Profile API — 10 admin endpoints (`/api/profiles/*`)
- **INF-001**: All backend APIs return JSON with `Result<T>` / `PagedResult<T>` envelope

## 15. Related Specifications

- `docs/superpowers/specs/2026-07-21-admin-spa-refactor-design.md` — prior admin SPA refactor
- `docs/superpowers/specs/2026-07-21-shared-components-sakai-alignment-design.md` — shared component design
- `docs/codebase/ARCHITECTURE.md` — overall architecture
- `docs/codebase/CONVENTIONS.md` — coding conventions
