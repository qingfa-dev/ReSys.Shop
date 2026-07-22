---
title: Admin SPA — Consistent List + Detail Page Pattern
version: 1.0
date_created: 2026-07-22
owner: Admin SPA team
tags: [design, app, admin-spa, vue, primevue]
---

# Introduction

Replace all 32 admin SPA stub pages with real list + detail pages following a
single consistent pattern across all 9 modules. Drop specialized page variants
(like `TreeManagerPage`, separate create pages, separate sub-entity list pages)
so every top-level entity follows the same `ListPage` + `DetailPage` convention.

## 1. Purpose & Scope

**Purpose:** Define the page model, routing pattern, component tree, sub-entity
management, and consistency rules for all admin SPA pages.

**Scope:** All 9 modules — Catalog, Inventory, Ordering, Payment, Shipping,
Location, Users, Profile, Reports. Every existing stub page is either replaced
or removed.

**Audience:** Frontend developers implementing the admin SPA pages.

**Assumptions:**
- PrimeVue v5, Tailwind v4, and the existing shared component suite are used
- Backend API endpoints already exist for all entities
- No new dependencies are added

## 2. Definitions

- **ListPage** — A page rendering a `DataTable` with search, filter, pagination, row actions
- **DetailPage** — A single page handling create, view, and edit modes via route-driven mode detection
- **Top-level entity** — A domain entity that has its own route and API CRUD endpoints
- **Sub-entity** — An entity owned by a parent, managed inline on the parent's DetailPage
- **MPTT** — Modified Preorder Tree Traversal (the nested-set hierarchy used by Taxons)

## 3. Requirements, Constraints & Guidelines

- **PAT-001**: Every top-level entity gets exactly 1 `ListPage` and 1 `DetailPage`
- **PAT-002**: Sub-entities are rendered inline on their parent's `DetailPage` as `<Fieldset>` sections. No separate routes.
- **PAT-003**: DetailPage detects mode from route: `/new` = create, `/:id` = view, `/:id/edit` = edit
- **PAT-004**: No specialized page types (TreeManager, separate Create pages, separate View pages)
- **PAT-005**: Pages use shared components directly. No generic composable layer — consistency via convention.
- **PAT-006**: Taxon hierarchy shown as flat DataTable with CSS depth indentation, not a Tree component
- **GUD-001**: Each module gets an API client module (`src/shared/api/catalog.ts`, etc.)
- **GUD-002**: Client-side form validation only, inline per page. No validation library.
- **CON-001**: Use existing `useToastNotify` for feedback, `useConfirm` for destructive actions
- **CON-002**: No new npm packages or dependencies

## 4. Entity-to-Page Mapping

### Complete page inventory

**Key:** L = ListPage, D = DetailPage, K = Keep existing (Dashboard), — = removed

### Catalog (7 existing → 7 new pages, 4 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| Product | `ProductListPage.vue` (K) | `ProductDetailPage.vue` (new) | `ProductCreatePage.vue` |
| Taxonomy | `TaxonomyListPage.vue` (new) | `TaxonomyDetailPage.vue` (new) | `TaxonTreeManagerPage.vue`, `TaxonListPage.vue` |
| OptionType | `OptionTypeListPage.vue` (K) | `OptionTypeDetailPage.vue` (new) | `OptionValueListPage.vue` |

**Sub-entities inline:** Variants, Prices, Images, Classifications on ProductDetailPage. Taxons on TaxonomyDetailPage. OptionValues on OptionTypeDetailPage.

### Inventory (7 existing → 7 new pages, 2 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| StockItem | `StockListPage.vue` (K) | — (view-only) | `StockImportPage.vue` |
| StockLocation | `LocationListPage.vue` (K) | `LocationDetailPage.vue` (new) | — |
| StockMovement | `MovementListPage.vue` (K) | — (read-only log) | — |
| StockTransfer | `TransferListPage.vue` (K) | `TransferDetailPage.vue` (new) | — |
| — | — | — | `UnitListPage.vue` (no backend entity) |

### Ordering (4 existing → 4 new pages, 1 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Dashboard | `DashboardPage.vue` (K) | — | — |
| Order | `OrderListPage.vue` (K) | `OrderDetailPage.vue` (new) | `OrderCreatePage.vue` |
| Fulfillment | `FulfillmentQueuePage.vue` (K) | — (read-only) | — |

### Payment (2 existing → 4 new pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| Payment | `PaymentListPage.vue` (K) | `PaymentDetailPage.vue` (new) |
| PaymentMethod | `PaymentMethodListPage.vue` (K) | `PaymentMethodDetailPage.vue` (new) |

### Shipping (2 existing → 4 new pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| ShippingMethod | `ShippingMethodListPage.vue` (K) | `ShippingMethodDetailPage.vue` (new) |
| ShippingRate | `ShippingRateListPage.vue` (K) | `ShippingRateDetailPage.vue` (new) |

### Location (2 existing → 4 new pages, 0 removed)

| Entity | ListPage | DetailPage |
|--------|----------|------------|
| Country | `CountryListPage.vue` (K) | `CountryDetailPage.vue` (new) |
| State | `StateListPage.vue` (K) | `StateDetailPage.vue` (new) |

### Users (5 existing → 8 new pages, 1 removed)

| Entity | ListPage | DetailPage | Removed |
|--------|----------|------------|---------|
| Staff | `StaffListPage.vue` (K) | `StaffDetailPage.vue` (new) | `StaffCreatePage.vue` |
| Customer | `CustomerListPage.vue` (K) | `CustomerDetailPage.vue` (new) | — |
| Role | `RoleListPage.vue` (K) | `RoleDetailPage.vue` (new) | — |
| Permission | `PermissionListPage.vue` (K) | `PermissionDetailPage.vue` (new) | — |

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
| Existing stub pages | 32 |
| Pages to keep (existing) | 24 |
| Pages to create (new DetailPage files) | 17 |
| Pages to remove | 8 |
| **Total resulting pages** | **41** |

**New DetailPage files to create (17):**

```
app/Admin/src/features/catalog/pages/ProductDetailPage.vue
app/Admin/src/features/catalog/pages/TaxonomyListPage.vue
app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue
app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue
app/Admin/src/features/inventory/pages/LocationDetailPage.vue
app/Admin/src/features/inventory/pages/TransferDetailPage.vue
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

## 5. Routing

### Pattern per entity

```
/module/entity              → XxxListPage.vue
/module/entity/new          → XxxDetailPage.vue  (mode: create)
/module/entity/:id          → XxxDetailPage.vue  (mode: view)
/module/entity/:id/edit     → XxxDetailPage.vue  (mode: edit)
```

4 routes, 2 page components per entity.

### Mode detection

```ts
const route = useRoute()
const id = computed(() => route.params.id as string | undefined)
const mode = computed(() =>
  !id.value ? 'create'
    : route.name?.toString().endsWith('.edit') ? 'edit'
    : 'view'
)
```

### Route files to update (8 of 9)

| Route File | Changes |
|-----------|---------|
| `catalog.routes.ts` | Replace all. Remove: products/create, taxa, taxonomies(old), option-values. Add: products/:id, products/:id/edit, taxonomies/new, taxonomies/:id, taxonomies/:id/edit, option-types/new, option-types/:id, option-types/:id/edit |
| `inventory.routes.ts` | Remove: stocks/import, units. Add: locations/new, locations/:id, locations/:id/edit, transfers/new, transfers/:id, transfers/:id/edit |
| `ordering.routes.ts` | Remove: orders/create. Add: orders/:id, orders/:id/edit |
| `payment.routes.ts` | Add: list/new, list/:id, list/:id/edit, methods/new, methods/:id, methods/:id/edit |
| `shipping.routes.ts` | Add: methods/new, methods/:id, methods/:id/edit, rates/new, rates/:id, rates/:id/edit |
| `location.routes.ts` | Add: countries/new, countries/:id, countries/:id/edit, states/new, states/:id, states/:id/edit |
| `users.routes.ts` | Remove: staff/create. Add: staff/new, staff/:id, staff/:id/edit, customers/new, customers/:id, customers/:id/edit, roles/new, roles/:id, roles/:id/edit, permissions/new, permissions/:id, permissions/:id/edit |
| `reports.routes.ts` | No changes |
| `profile.routes.ts` | No structural changes |

## 6. Component Tree

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

Clicking "+ Add" or row "Edit" opens an inline slideover (PrimeVue `Drawer`)
with the sub-entity form. On save, the sub-table refreshes without navigation.

Nested sub-entities (e.g., Variant → Prices, Images): use tabs inside the slideover.

## 7. Taxon Depth Indentation

Taxons are rendered as a flat array in `TaxonomyDetailPage`'s taxons sub-table,
sorted by `lft` ASC. Visual indent = `depth × 1.5rem` with tree-line `::before`
pseudo on the first column. No `Tree` or `TreeTable` component.

| depth | display       |
|-------|---------------|
| 0     | Clothing      |
| 1     | ├─ Men's      |
| 2     | ├── Tops      |
| 2     | ├── Bottoms    |
| 1     | ├─ Women's    |

## 8. Menu Changes

**Entries to remove (8):**

| Menu Entry | Route Name | Reason |
|-----------|-----------|--------|
| "Add Product" | `catalog.products.create` | Merged into ProductDetailPage create mode |
| "All Categories" | `catalog.taxa.list` | TaxonListPage removed; taxons on TaxonomyDetailPage |
| "Manager" | `catalog.taxonomies.list` (old) | TreeManagerPage replaced; route now points to TaxonomyListPage |
| "Values" | `catalog.option-values.list` | OptionValueListPage removed; values on OptionTypeDetailPage |
| "Import" | `inventory.stocks.import` | Becomes action button on StockListPage |
| "Stock Units" | `inventory.units.list` | No backend entity |
| "Create Order" | `ordering.orders.create` | Merged into OrderDetailPage create mode |
| "Invite Staff" | `users.staff.create` | Merged into StaffDetailPage create mode |

**Entry to rename:**

"Manage" → replace `catalog.taxonomies.list` target from `TaxonTreeManagerPage` to `TaxonomyListPage`.

## 9. API & Error Handling

### HTTP wrapper

New `src/shared/api/http.ts`:
```ts
async function api<T>(url, init?): Promise<T>  // fetch + JSON + error normalize
```

Throws `ApiError { status, message }` on non-2xx. Pages set reactive `error` state
→ `ErrorState` component renders with retry.

### Domain API modules

```
src/shared/api/catalog.ts       # Products, Taxonomies, OptionTypes endpoints
src/shared/api/inventory.ts     # StockItems, Locations, Movements, Transfers
src/shared/api/ordering.ts      # Orders, Fulfillment
src/shared/api/payment.ts       # Payments, PaymentMethods
src/shared/api/shipping.ts      # ShippingMethods, ShippingRates
src/shared/api/location.ts      # Countries, States
src/shared/api/users.ts         # Staff, Customers, Roles, Permissions
src/shared/api/profile.ts       # Profile, Addresses
src/shared/api/reports.ts       # Dashboard stats
```

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

## 10. Testing

- **Shared component tests** (existing): `DataTable`, `FormField`, `TableToolbar`, `PageHeader`
- **Page smoke tests**: mount without crashing, render `PageHeader` with correct title, handle loading / empty / error states
- **No integration tests**: API calls mocked

## 11. Acceptance Criteria

- **AC-001**: Given any module, every top-level entity has a working `ListPage` with `DataTable`, search, pagination, and create/edit/delete actions.
- **AC-002**: Given any top-level entity, its `DetailPage` opens in view mode at `/:id`, switches to edit mode at `/:id/edit`, and opens blank form in create mode at `/new`.
- **AC-003**: Given a `ProductDetailPage`, Variants, Prices, Images, Classifications appear as inline sub-tables within `<Fieldset>` sections.
- **AC-004**: Given a `TaxonomyDetailPage`, Taxons appear as a flat indented table sorted by `lft`, with no `Tree` component.
- **AC-005**: No `ProductCreatePage`, `TaxonListPage`, `TreeManagerPage`, `OptionValueListPage`, `StockImportPage`, `UnitListPage`, `OrderCreatePage`, or `StaffCreatePage` files exist.
- **AC-006**: Given a destructive action (delete entity, delete sub-entity), a confirmation dialog appears using `useConfirm`.
- **AC-007**: Given a successful save or delete, a toast notification appears using `useToastNotify`.
- **AC-008**: All 8 dropped menu entries are removed from `admin-menu.config.ts`.

## 12. Rationale & Context

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

## 13. Dependencies & External Integrations

- **PLT-001**: Vue 3.5 + TypeScript 6 — runtime platform
- **PLT-002**: PrimeVue 5 + Aura preset — component library and theming
- **PLT-003**: Tailwind v4 — utility CSS
- **SVC-001**: Backend Catalog API (`/api/catalog/*`) — must provide CRUD endpoints for Products, Taxonomies, OptionTypes
- **SVC-002**: Backend Inventory API — StockItems, StockLocations, StockMovements, StockTransfers
- **SVC-003**: Backend Ordering API — Orders, Fulfillment
- **SVC-004**: Backend Payment API — Payments, PaymentMethods
- **SVC-005**: Backend Shipping API — ShippingMethods, ShippingRates
- **SVC-006**: Backend Location API — Countries, States
- **SVC-007**: Backend Identity API — Staff, Customers, Roles, Permissions
- **SVC-008**: Backend Profile API — Profile, Addresses
- **INF-001**: All backend APIs must accept and return JSON with consistent `Result<T>` envelope

## 14. Related Specifications

- `docs/superpowers/specs/2026-07-21-admin-spa-refactor-design.md` — prior admin SPA refactor
- `docs/superpowers/specs/2026-07-21-shared-components-sakai-alignment-design.md` — shared component design
- `docs/codebase/ARCHITECTURE.md` — overall architecture
- `docs/codebase/CONVENTIONS.md` — coding conventions
