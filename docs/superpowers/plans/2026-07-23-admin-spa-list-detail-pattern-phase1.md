# Admin SPA List-Detail Pattern — Phase 1: Infrastructure + Catalog

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development
> (recommended) or superpowers:executing-plans to implement this plan task-by-task.
> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure all 9 feature module directories to the `auth/` pattern, update all
route files and menu config, then build real Catalog module pages (7 pages) establishing
the list+detail pattern for all subsequent modules.

**Architecture:** Convention over abstraction — pages use existing shared components
directly (`DataTable`, `PageHeader`, `FormField`, `TableToolbar`, etc.). Each entity
gets 1 ListPage + 1 DetailPage (handles create/view/edit via route-driven mode).
Sub-entities rendered as `<Fieldset>` sections on parent DetailPage. API calls use
existing Axios `apiClient` + `resultToMapped`/`pagedResultToMapped` from `src/shared/api/`.

**Tech Stack:** Vue 3.5, TypeScript 6, PrimeVue 5, Tailwind v4, Axios (existing apiClient)

## Global Constraints

- PrimeVue v5 + Aura preset — no new component library
- Tailwind v4 — no new CSS framework
- Existing shared components at `src/shared/components/` — reuse all
- Existing API infra at `src/shared/api/` — Axios client, interceptors, mappers
- `src/shared/models/` — `Result<T>`, `PagedResult<T>`, `PaginationMeta` types
- No new npm dependencies
- Route `:id` param is GUID string
- `useToastNotify` for feedback, `useConfirm` for destructive actions
- Each module directory matches `auth/` layout: api/, components/, composables/, models/, pages/, store/, utils/, routes.ts
- Spec: `spec/design-admin-spa-list-detail-pattern.md`

---

## File Structure

### Created

```
app/Admin/src/features/catalog/api/
app/Admin/src/features/catalog/api/index.ts
app/Admin/src/features/catalog/api/products.ts
app/Admin/src/features/catalog/api/taxonomies.ts
app/Admin/src/features/catalog/api/optionTypes.ts
app/Admin/src/features/catalog/components/
app/Admin/src/features/catalog/composables/
app/Admin/src/features/catalog/models/
app/Admin/src/features/catalog/models/Product.ts
app/Admin/src/features/catalog/models/Taxonomy.ts
app/Admin/src/features/catalog/models/OptionType.ts
app/Admin/src/features/catalog/models/index.ts
app/Admin/src/features/catalog/store/
app/Admin/src/features/catalog/utils/
app/Admin/src/features/catalog/pages/ProductDetailPage.vue
app/Admin/src/features/catalog/pages/TaxonomyListPage.vue
app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue
app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue

app/Admin/src/features/inventory/api/
app/Admin/src/features/inventory/components/
app/Admin/src/features/inventory/composables/
app/Admin/src/features/inventory/models/
app/Admin/src/features/inventory/store/
app/Admin/src/features/inventory/utils/

app/Admin/src/features/ordering/api/
app/Admin/src/features/ordering/components/
app/Admin/src/features/ordering/composables/
app/Admin/src/features/ordering/models/
app/Admin/src/features/ordering/store/
app/Admin/src/features/ordering/utils/

app/Admin/src/features/payment/api/
app/Admin/src/features/payment/components/
app/Admin/src/features/payment/composables/
app/Admin/src/features/payment/models/
app/Admin/src/features/payment/store/
app/Admin/src/features/payment/utils/

app/Admin/src/features/shipping/api/
app/Admin/src/features/shipping/components/
app/Admin/src/features/shipping/composables/
app/Admin/src/features/shipping/models/
app/Admin/src/features/shipping/store/
app/Admin/src/features/shipping/utils/

app/Admin/src/features/location/api/
app/Admin/src/features/location/components/
app/Admin/src/features/location/composables/
app/Admin/src/features/location/models/
app/Admin/src/features/location/store/
app/Admin/src/features/location/utils/

app/Admin/src/features/users/api/
app/Admin/src/features/users/components/
app/Admin/src/features/users/composables/
app/Admin/src/features/users/models/
app/Admin/src/features/users/store/
app/Admin/src/features/users/utils/

app/Admin/src/features/profile/api/
app/Admin/src/features/profile/components/
app/Admin/src/features/profile/composables/
app/Admin/src/features/profile/models/
app/Admin/src/features/profile/store/
app/Admin/src/features/profile/utils/

app/Admin/src/features/reports/api/
app/Admin/src/features/reports/components/
app/Admin/src/features/reports/composables/
app/Admin/src/features/reports/models/
app/Admin/src/features/reports/store/
app/Admin/src/features/reports/utils/

app/Admin/src/shared/api/services/catalog-api.ts
app/Admin/src/shared/api/services/paged-query.ts
app/Admin/src/shared/models/catalog.ts
app/Admin/src/shared/models/inventory.ts
app/Admin/src/shared/models/ordering.ts
app/Admin/src/shared/models/location.ts
app/Admin/src/shared/models/identity.ts
```

### Modified

```
app/Admin/src/app/config/admin-menu.config.ts          # remove 6 menu entries, rename 2
app/Admin/src/app/routes/catalog.routes.ts              # full rewrite
app/Admin/src/app/routes/inventory.routes.ts            # remove units & import, add detail routes
app/Admin/src/app/routes/ordering.routes.ts             # remove create, add detail routes
app/Admin/src/app/routes/payment.routes.ts              # normalize path, add detail routes
app/Admin/src/app/routes/shipping.routes.ts             # add detail routes
app/Admin/src/app/routes/location.routes.ts             # add detail routes
app/Admin/src/app/routes/users.routes.ts                # remove staff/create, add detail routes
app/Admin/src/app/routes/profile.routes.ts              # normalize route names

app/Admin/src/features/catalog/pages/DashboardPage.vue          # replace stub
app/Admin/src/features/catalog/pages/ProductListPage.vue        # replace stub
app/Admin/src/features/catalog/pages/OptionTypeListPage.vue     # replace stub
```

### Deleted

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

---

### Task 1: Restructure all 9 module directories to auth/ pattern

**Files:**
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/api/` (empty directories)
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/components/`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/composables/`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/models/`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/store/`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/utils/`

**Interfaces:**
- Consumes: nothing
- Produces: 7 empty subdirectories per module, each with `.gitkeep`

- [ ] **Step 1: Create empty directories with .gitkeep files**

```bash
for mod in catalog inventory ordering payment shipping location users profile reports; do
  mkdir -p app/Admin/src/features/$mod/{api,components,composables,models,store,utils}
  for dir in api components composables models store utils; do
    touch app/Admin/src/features/$mod/$dir/.gitkeep
  done
done
```

- [ ] **Step 2: Verify structure**

```bash
ls -R app/Admin/src/features/catalog/
# Expected: api/ components/ composables/ models/ pages/ store/ utils/
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/{api,components,composables,models,store,utils}/
git commit -m "chore: add auth-style subdirectories to all feature modules"
```

---

### Task 2: Remove 8 deprecated page files

**Files:**
- Delete: `app/Admin/src/features/catalog/pages/ProductCreatePage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonListPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/OptionValueListPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/StockImportPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/UnitListPage.vue`
- Delete: `app/Admin/src/features/ordering/pages/OrderCreatePage.vue`
- Delete: `app/Admin/src/features/users/pages/StaffCreatePage.vue`

**Interfaces:**
- Consumes: nothing
- Produces: removed files

- [ ] **Step 1: Delete files**

```bash
rm app/Admin/src/features/catalog/pages/ProductCreatePage.vue
rm app/Admin/src/features/catalog/pages/TaxonListPage.vue
rm app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue
rm app/Admin/src/features/catalog/pages/OptionValueListPage.vue
rm app/Admin/src/features/inventory/pages/StockImportPage.vue
rm app/Admin/src/features/inventory/pages/UnitListPage.vue
rm app/Admin/src/features/ordering/pages/OrderCreatePage.vue
rm app/Admin/src/features/users/pages/StaffCreatePage.vue
```

- [ ] **Step 2: Verify files removed**

```bash
ls app/Admin/src/features/catalog/pages/ProductCreatePage.vue 2>&1
# Expected: No such file or directory
```

- [ ] **Step 3: Commit**

```bash
git add -A app/Admin/src/features/
git commit -m "chore: remove 8 deprecated page files (create pages, tree manager, etc.)"
```

---

### Task 3: Update Catalog route file

**Files:**
- Modify: `app/Admin/src/app/routes/catalog.routes.ts`

**Interfaces:**
- Consumes: nothing (current file reads from old paths)
- Produces: `catalogRoutes` RouteRecordRaw with 14 child routes

- [ ] **Step 1: Rewrite catalog routes**

Replace the content of `app/Admin/src/app/routes/catalog.routes.ts` with:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  children: [
    { path: '', redirect: { name: 'catalog.dashboard' } },
    {
      path: 'dashboard',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/pages/DashboardPage.vue'),
    },
    {
      path: 'products',
      name: 'catalog.products.list',
      component: () => import('@/features/catalog/pages/ProductListPage.vue'),
    },
    {
      path: 'products/new',
      name: 'catalog.products.create',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id',
      name: 'catalog.products.view',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id/edit',
      name: 'catalog.products.edit',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'taxonomies',
      name: 'catalog.taxonomies.list',
      component: () => import('@/features/catalog/pages/TaxonomyListPage.vue'),
    },
    {
      path: 'taxonomies/new',
      name: 'catalog.taxonomies.create',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id',
      name: 'catalog.taxonomies.view',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id/edit',
      name: 'catalog.taxonomies.edit',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'option-types',
      name: 'catalog.option-types.list',
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
    },
    {
      path: 'option-types/new',
      name: 'catalog.option-types.create',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id',
      name: 'catalog.option-types.view',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id/edit',
      name: 'catalog.option-types.edit',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 2: Verify route file is valid**

```bash
cd app/Admin && pnpm run typecheck 2>&1 | grep -i "catalog.routes" || echo "No catalog route errors"
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/routes/catalog.routes.ts
git commit -m "feat: rewrite catalog routes with list-detail pattern (14 routes)"
```

---

### Task 4: Update remaining 8 route files

**Files:**
- Modify: `app/Admin/src/app/routes/inventory.routes.ts`
- Modify: `app/Admin/src/app/routes/ordering.routes.ts`
- Modify: `app/Admin/src/app/routes/payment.routes.ts`
- Modify: `app/Admin/src/app/routes/shipping.routes.ts`
- Modify: `app/Admin/src/app/routes/location.routes.ts`
- Modify: `app/Admin/src/app/routes/users.routes.ts`
- Modify: `app/Admin/src/app/routes/profile.routes.ts`

**Interfaces:**
- Consumes: nothing except existing component imports
- Produces: updated route exports per spec Sections 5-6

- [ ] **Step 1: Rewrite inventory routes** — replace `app/Admin/src/app/routes/inventory.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  children: [
    { path: '', redirect: { name: 'inventory.dashboard' } },
    {
      path: 'dashboard',
      name: 'inventory.dashboard',
      component: () => import('@/features/inventory/pages/DashboardPage.vue'),
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('@/features/inventory/pages/StockListPage.vue'),
    },
    {
      path: 'stocks/new',
      name: 'inventory.stocks.create',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id',
      name: 'inventory.stocks.view',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id/edit',
      name: 'inventory.stocks.edit',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'locations',
      name: 'inventory.locations.list',
      component: () => import('@/features/inventory/pages/LocationListPage.vue'),
    },
    {
      path: 'locations/new',
      name: 'inventory.locations.create',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id',
      name: 'inventory.locations.view',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id/edit',
      name: 'inventory.locations.edit',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('@/features/inventory/pages/MovementListPage.vue'),
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('@/features/inventory/pages/TransferListPage.vue'),
    },
    {
      path: 'transfers/new',
      name: 'inventory.transfers.create',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.view',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id/edit',
      name: 'inventory.transfers.edit',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'reservations',
      name: 'inventory.reservations.list',
      component: () => import('@/features/inventory/pages/StockReservationListPage.vue'),
    },
  ],
}
```

- [ ] **Step 2: Rewrite ordering routes** — replace `app/Admin/src/app/routes/ordering.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  children: [
    { path: '', redirect: { name: 'ordering.dashboard' } },
    {
      path: 'dashboard',
      name: 'ordering.dashboard',
      component: () => import('@/features/ordering/pages/DashboardPage.vue'),
    },
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/pages/OrderListPage.vue'),
    },
    {
      path: 'orders/new',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.view',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id/edit',
      name: 'ordering.orders.edit',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/pages/FulfillmentQueuePage.vue'),
    },
  ],
}
```

- [ ] **Step 3: Rewrite payment routes** — replace `app/Admin/src/app/routes/payment.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  children: [
    { path: '', redirect: { name: 'payment.payments.list' } },
    {
      path: 'payments',
      name: 'payment.payments.list',
      component: () => import('@/features/payment/pages/PaymentListPage.vue'),
    },
    {
      path: 'payments/:id',
      name: 'payment.payments.view',
      component: () => import('@/features/payment/pages/PaymentDetailPage.vue'),
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('@/features/payment/pages/PaymentMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: 'payment.methods.create',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'payment.methods.view',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 4: Rewrite shipping routes** — replace `app/Admin/src/app/routes/shipping.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  children: [
    { path: '', redirect: { name: 'shipping.methods.list' } },
    {
      path: 'methods',
      name: 'shipping.methods.list',
      component: () => import('@/features/shipping/pages/ShippingMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: 'shipping.methods.create',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'shipping.methods.view',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'shipping.methods.edit',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('@/features/shipping/pages/ShippingRateListPage.vue'),
    },
    {
      path: 'rates/new',
      name: 'shipping.rates.create',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id',
      name: 'shipping.rates.view',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id/edit',
      name: 'shipping.rates.edit',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 5: Rewrite location routes** — replace `app/Admin/src/app/routes/location.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  children: [
    { path: '', redirect: { name: 'location.countries.list' } },
    {
      path: 'countries',
      name: 'location.countries.list',
      component: () => import('@/features/location/pages/CountryListPage.vue'),
    },
    {
      path: 'countries/new',
      name: 'location.countries.create',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id',
      name: 'location.countries.view',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id/edit',
      name: 'location.countries.edit',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('@/features/location/pages/StateListPage.vue'),
    },
    {
      path: 'states/new',
      name: 'location.states.create',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id',
      name: 'location.states.view',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id/edit',
      name: 'location.states.edit',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 6: Rewrite users routes** — replace `app/Admin/src/app/routes/users.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  children: [
    { path: '', redirect: { name: 'users.staff.list' } },
    {
      path: 'staff',
      name: 'users.staff.list',
      component: () => import('@/features/users/pages/StaffListPage.vue'),
    },
    {
      path: 'staff/new',
      name: 'users.staff.create',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id',
      name: 'users.staff.view',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id/edit',
      name: 'users.staff.edit',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'customers',
      name: 'users.customers.list',
      component: () => import('@/features/users/pages/CustomerListPage.vue'),
    },
    {
      path: 'customers/new',
      name: 'users.customers.create',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id',
      name: 'users.customers.view',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id/edit',
      name: 'users.customers.edit',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'roles',
      name: 'users.roles.list',
      component: () => import('@/features/users/pages/RoleListPage.vue'),
    },
    {
      path: 'roles/new',
      name: 'users.roles.create',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id',
      name: 'users.roles.view',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id/edit',
      name: 'users.roles.edit',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'permissions',
      name: 'users.permissions.list',
      component: () => import('@/features/users/pages/PermissionListPage.vue'),
    },
    {
      path: 'permissions/:id',
      name: 'users.permissions.view',
      component: () => import('@/features/users/pages/PermissionDetailPage.vue'),
    },
  ],
}
```

Note: Permission has no `new`/`edit` routes — backend only supports `GET /api/identity/permissions` (read-only list).

- [ ] **Step 7: Rewrite profile routes** — replace `app/Admin/src/app/routes/profile.routes.ts`

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const profileRoutes: RouteRecordRaw = {
  path: 'profile',
  children: [
    { path: '', name: 'profile.view', component: () => import('@/features/profile/pages/ProfilePage.vue') },
    { path: 'addresses', name: 'profile.addresses', component: () => import('@/features/profile/pages/AddressListPage.vue') },
  ],
}
```

- [ ] **Step 8: Verify typecheck passes**

```bash
cd app/Admin && pnpm run typecheck
# Expected: no errors. If errors, fix import paths for pages not yet created (they will 404 at runtime but typecheck should pass since imports are lazy)
```

- [ ] **Step 9: Commit**

```bash
git add app/Admin/src/app/routes/
git commit -m "feat: update all route files with list-detail pattern"
```

---

### Task 5: Update admin menu config

**Files:**
- Modify: `app/Admin/src/app/config/admin-menu.config.ts`

**Interfaces:**
- Consumes: nothing
- Produces: updated `adminMenuConfig` array

- [ ] **Step 1: Apply menu changes**

In `admin-menu.config.ts`:

Remove these menu entries:
1. "Add Product" (child of "Products" group) — routes to old `catalog.products.create`
2. "All Categories" (child of "Categories" group) — routes to `catalog.taxa.list`
3. "Manager" (child of "Categories" group) — routes to old taxonomies list → rename to "All Taxonomies" pointing to `catalog.taxonomies.list`
4. "Values" (child of "Option Types" group) — routes to `catalog.option-values.list`
5. "Import" (under Inventory) — routes to `inventory.stocks.import`
6. "Stock Units" (under Inventory) — routes to `inventory.units.list`
7. "Create Order" (child of "All Orders" group) — routes to `ordering.orders.create`
8. "Invite Staff" (child of "Staff" group) — routes to `users.staff.create`

Change the "Categories" submenu to:
```typescript
{
  label: 'Categories',
  icon: 'pi pi-fw pi-sitemap',
  permission: 'Catalog.Taxonomies',
  items: [
    { label: 'All Taxonomies', icon: 'pi pi-fw pi-tags', to: { name: 'catalog.taxonomies.list' } },
  ],
},
```

Change "Option Types" submenu to:
```typescript
{
  label: 'Option Types',
  icon: 'pi pi-fw pi-list',
  permission: 'Catalog.OptionTypes',
  items: [
    { label: 'All Types', icon: 'pi pi-fw pi-list', to: { name: 'catalog.option-types.list' } },
  ],
},
```

Keep "Products" submenu with only:
```typescript
{
  label: 'Products',
  icon: 'pi pi-fw pi-shopping-bag',
  permission: 'Catalog.Products',
  items: [
    { label: 'All Products', icon: 'pi pi-fw pi-list', to: { name: 'catalog.products.list' } },
  ],
},
```

Remove "Import", "Stock Units" from Inventory. Remove "Create Order" child. Remove "Invite Staff" child.

- [ ] **Step 2: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
# Expected: no errors related to menu
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/config/admin-menu.config.ts
git commit -m "chore: update admin menu — remove deprecated entries, rename taxonomy manager"
```

---

### Task 6: Create Catalog shared models (TypeScript interfaces)

**Files:**
- Create: `app/Admin/src/features/catalog/models/Product.ts`
- Create: `app/Admin/src/features/catalog/models/Taxonomy.ts`
- Create: `app/Admin/src/features/catalog/models/OptionType.ts`
- Create: `app/Admin/src/features/catalog/models/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `ProductResponse`, `ProductRequest`, `ProductListParams`, `TaxonomyResponse`, etc.

- [ ] **Step 1: Write Product models** — `app/Admin/src/features/catalog/models/Product.ts`

```typescript
export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string | null
  status: ProductStatus
  styleCode: string | null
  seasonName: string | null
  department: string | null
  genderTarget: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  createdAt: string
  updatedAt: string
}

export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductRequest {
  name: string
  slug: string
  description?: string | null
  status?: ProductStatus
  styleCode?: string | null
  seasonName?: string | null
  department?: string | null
  genderTarget?: string | null
  metaTitle?: string | null
  metaDescription?: string | null
  metaKeywords?: string | null
  availableOn?: string | null
  discontinueOn?: string | null
  materialComposition?: string | null
  careInstructions?: string | null
  fitNotes?: string | null
}

export interface ProductListParams {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  status?: ProductStatus
}
```

- [ ] **Step 2: Write Taxonomy models** — `app/Admin/src/features/catalog/models/Taxonomy.ts`

```typescript
export interface TaxonomyResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  createdAt: string
  updatedAt: string
}

export interface TaxonomyRequest {
  name: string
  presentation?: string | null
  position?: number
}

export interface TaxonomyListParams {
  page?: number
  pageSize?: number
  search?: string
}

export interface TaxonResponse {
  id: string
  name: string
  presentation: string | null
  description: string | null
  slug: string
  position: number
  depth: number
  lft: number
  rgt: number
  childrenCount: number
  hideFromNav: boolean
  automatic: boolean
  taxonomyId: string
  parentId: string | null
  createdAt: string
  updatedAt: string
}

export interface TaxonRequest {
  name: string
  presentation?: string | null
  description?: string | null
  slug?: string
  position?: number
  hideFromNav?: boolean
  automatic?: boolean
  parentId?: string | null
}
```

- [ ] **Step 3: Write OptionType models** — `app/Admin/src/features/catalog/models/OptionType.ts`

```typescript
export interface OptionTypeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
  createdAt: string
  updatedAt: string
}

export interface OptionTypeRequest {
  name: string
  presentation?: string | null
  position?: number
  filterable?: boolean
}

export interface OptionTypeListParams {
  page?: number
  pageSize?: number
  search?: string
}

export interface OptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
}

export interface OptionValueRequest {
  name: string
  presentation?: string | null
  position?: number
}
```

- [ ] **Step 4: Write barrel export** — `app/Admin/src/features/catalog/models/index.ts`

```typescript
export * from './Product'
export * from './Taxonomy'
export * from './OptionType'
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/models/
git commit -m "feat: add catalog TypeScript model interfaces"
```

---

### Task 7: Create Catalog API services

**Files:**
- Create: `app/Admin/src/features/catalog/api/products.ts`
- Create: `app/Admin/src/features/catalog/api/taxonomies.ts`
- Create: `app/Admin/src/features/catalog/api/optionTypes.ts`
- Create: `app/Admin/src/features/catalog/api/index.ts`

**Interfaces:**
- Consumes: `apiClient` from `@/shared/api/client`, `resultToMapped`/`pagedResultToMapped` from `@/shared/api/utils/result.mapper`, model types from `../models`
- Produces: typed async functions returning `MappedResult<T>`

- [ ] **Step 1: Write products API** — `app/Admin/src/features/catalog/api/products.ts`

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type { ProductResponse, ProductRequest, ProductListParams } from '../models/Product'

export async function getProducts(
  params: ProductListParams = {},
): Promise<MappedResult<ProductResponse[]> & { meta?: PaginationMeta }> {
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

export async function updateProduct(
  id: string,
  data: ProductRequest,
): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteProduct(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}
```

- [ ] **Step 2: Write taxonomies API** — `app/Admin/src/features/catalog/api/taxonomies.ts`

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  TaxonomyResponse,
  TaxonomyRequest,
  TaxonomyListParams,
  TaxonResponse,
  TaxonRequest,
} from '../models/Taxonomy'

export async function getTaxonomies(
  params: TaxonomyListParams = {},
): Promise<MappedResult<TaxonomyResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<TaxonomyResponse>>('/catalog/taxonomies', { params })
  return pagedResultToMapped(res.data)
}

export async function getTaxonomy(id: string): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.get<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function createTaxonomy(data: TaxonomyRequest): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.post<Result<TaxonomyResponse>>('/catalog/taxonomies', data)
  return resultToMapped(res.data)
}

export async function updateTaxonomy(
  id: string,
  data: TaxonomyRequest,
): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.put<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteTaxonomy(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function getTaxons(
  taxonomyId: string,
): Promise<MappedResult<TaxonResponse[]>> {
  const res = await apiClient.get<Result<TaxonResponse[]>>(
    `/catalog/taxonomies/${taxonomyId}/taxons`,
  )
  return resultToMapped(res.data)
}

export async function createTaxon(
  taxonomyId: string,
  data: TaxonRequest,
): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.post<Result<TaxonResponse>>(
    `/catalog/taxonomies/${taxonomyId}/taxons`,
    data,
  )
  return resultToMapped(res.data)
}

export async function updateTaxon(
  taxonomyId: string,
  id: string,
  data: TaxonRequest,
): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.put<Result<TaxonResponse>>(
    `/catalog/taxonomies/${taxonomyId}/taxons/${id}`,
    data,
  )
  return resultToMapped(res.data)
}

export async function deleteTaxon(taxonomyId: string, id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(
    `/catalog/taxonomies/${taxonomyId}/taxons/${id}`,
  )
  return resultToMapped(res.data)
}
```

- [ ] **Step 3: Write optionTypes API** — `app/Admin/src/features/catalog/api/optionTypes.ts`

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  OptionTypeResponse,
  OptionTypeRequest,
  OptionTypeListParams,
  OptionValueResponse,
  OptionValueRequest,
} from '../models/OptionType'

export async function getOptionTypes(
  params: OptionTypeListParams = {},
): Promise<MappedResult<OptionTypeResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<OptionTypeResponse>>('/catalog/option-types', {
    params,
  })
  return pagedResultToMapped(res.data)
}

export async function getOptionType(id: string): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.get<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function createOptionType(
  data: OptionTypeRequest,
): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.post<Result<OptionTypeResponse>>('/catalog/option-types', data)
  return resultToMapped(res.data)
}

export async function updateOptionType(
  id: string,
  data: OptionTypeRequest,
): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.put<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteOptionType(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function getOptionValues(
  optionTypeId: string,
): Promise<MappedResult<OptionValueResponse[]>> {
  const res = await apiClient.get<Result<OptionValueResponse[]>>(
    `/catalog/option-types/${optionTypeId}/values`,
  )
  return resultToMapped(res.data)
}

export async function createOptionValue(
  optionTypeId: string,
  data: OptionValueRequest,
): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.post<Result<OptionValueResponse>>(
    `/catalog/option-types/${optionTypeId}/values`,
    data,
  )
  return resultToMapped(res.data)
}

export async function updateOptionValue(
  optionTypeId: string,
  id: string,
  data: OptionValueRequest,
): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.put<Result<OptionValueResponse>>(
    `/catalog/option-types/${optionTypeId}/values/${id}`,
    data,
  )
  return resultToMapped(res.data)
}

export async function deleteOptionValue(
  optionTypeId: string,
  id: string,
): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(
    `/catalog/option-types/${optionTypeId}/values/${id}`,
  )
  return resultToMapped(res.data)
}
```

- [ ] **Step 4: Write barrel export** — `app/Admin/src/features/catalog/api/index.ts`

```typescript
export * as productsApi from './products'
export * as taxonomiesApi from './taxonomies'
export * as optionTypesApi from './optionTypes'
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/api/
git commit -m "feat: add catalog API service functions"
```

---

### Task 8: Build ProductListPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`

**Interfaces:**
- Consumes: `productsApi` from `../api`, `ProductResponse` from `../models`, shared components from `@/shared/components/`
- Produces: working product list with search, pagination, row actions

- [ ] **Step 1: Write ProductListPage** — replace the stub

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getProducts, deleteProduct } from '../api/products'
import type { ProductResponse, ProductStatus } from '../models/Product'

const router = useRouter()
const confirm = useConfirm()
const toast = useToastNotify()

const items = ref<ProductResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'slug', header: 'Slug' },
  { field: 'status', header: 'Status' },
  { field: 'department', header: 'Department' },
  { field: 'createdAt', header: 'Created' },
]

async function fetchProducts() {
  loading.value = true
  error.value = null
  const result = await getProducts({
    page: page.value,
    pageSize: pageSize.value,
    search: search.value || undefined,
  })
  if (result.success) {
    items.value = result.data
    totalCount.value = result.meta?.totalCount ?? 0
  } else {
    error.value = result.error.message ?? 'Failed to load products'
  }
  loading.value = false
}

function statusSeverity(status: ProductStatus) {
  return status === 'Active' ? 'success' : status === 'Draft' ? 'warn' : 'secondary'
}

function goToCreate() {
  router.push({ name: 'catalog.products.create' })
}

function goToView(id: string) {
  router.push({ name: 'catalog.products.view', params: { id } })
}

function goToEdit(id: string) {
  router.push({ name: 'catalog.products.edit', params: { id } })
}

async function onDelete(id: string) {
  const confirmed = await confirm({
    target: 'this product',
    onAccept: async () => {
      const result = await deleteProduct(id)
      if (result.success) {
        toast.success('Product deleted')
        await fetchProducts()
      } else {
        toast.error(result.error.message ?? 'Failed to delete')
      }
    },
  })
}

function onSearch() {
  page.value = 1
  fetchProducts()
}

function onPageChange(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  fetchProducts()
}

onMounted(() => {
  fetchProducts()
})
</script>

<template>
  <div>
    <PageHeader title="Products" subtitle="Manage product catalog" />
    <TableToolbar
      v-model:search="search"
      search-placeholder="Search products..."
      create-label="Add Product"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="5" />
    <ErrorState v-else-if="error" :message="error" @retry="fetchProducts" />
    <EmptyState
      v-else-if="items.length === 0"
      title="No products"
      description="Create your first product to get started."
    >
      <button @click="goToCreate">Add Product</button>
    </EmptyState>
    <DataTable
      v-else
      :value="items"
      :columns="columns"
      :loading="loading"
      :total-records="totalCount"
      :rows="pageSize"
      :first="(page - 1) * pageSize"
      lazy
      paginator
      striped-rows
      @page="onPageChange"
      @row-click="(e: { data: ProductResponse }) => goToView(e.data.id)"
    >
      <template #body-status="{ data }">
        <StatusTag :value="data.status" :severity="statusSeverity(data.status)" />
      </template>
      <template #body-actions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: 'Edit', icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
# Expected: no TypeScript errors (may warn about unused imports — fix if so)
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/pages/ProductListPage.vue
git commit -m "feat: implement ProductListPage with search, pagination, row actions"
```

---

### Task 9: Build ProductDetailPage

**Files:**
- Create: `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`

**Interfaces:**
- Consumes: `productsApi`, `ProductResponse`/`ProductRequest` from models, shared components
- Produces: detail page with create/view/edit modes, form fields for all Product properties

- [ ] **Step 1: Write ProductDetailPage**

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getProduct, createProduct, updateProduct } from '../api/products'
import type { ProductResponse, ProductRequest, ProductStatus } from '../models/Product'

const route = useRoute()
const router = useRouter()
const toast = useToastNotify()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const form = ref<ProductRequest & { status: ProductStatus }>({
  name: '',
  slug: '',
  description: null,
  status: 'Draft',
  styleCode: null,
  seasonName: null,
  department: null,
  genderTarget: null,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
})

const formErrors = ref<Record<string, string>>({})

const title = computed(() => {
  if (mode.value === 'create') return 'Create Product'
  if (mode.value === 'edit') return `Edit: ${form.value.name || 'Product'}`
  return form.value.name || 'Product Detail'
})

function validate(): boolean {
  formErrors.value = {}
  if (!form.value.name.trim()) formErrors.value.name = 'Required'
  if (!form.value.slug.trim()) formErrors.value.slug = 'Required'
  return Object.keys(formErrors.value).length === 0
}

async function load() {
  if (!id.value) return
  loading.value = true
  error.value = null
  const result = await getProduct(id.value)
  if (result.success) {
    form.value = { ...result.data }
  } else {
    error.value = result.error.message ?? 'Failed to load product'
  }
  loading.value = false
}

async function save() {
  if (!validate()) return
  saving.value = true
  const data: ProductRequest = { ...form.value }
  const result = id.value
    ? await updateProduct(id.value, data)
    : await createProduct(data)
  saving.value = false
  if (result.success) {
    toast.success(id.value ? 'Product updated' : 'Product created')
    if (mode.value === 'create') {
      router.replace({ name: 'catalog.products.view', params: { id: result.data.id } })
    } else {
      router.replace({ name: 'catalog.products.view', params: { id: id.value } })
    }
  } else {
    toast.error(result.error.message ?? 'Save failed')
  }
}

function cancel() {
  if (id.value) {
    router.push({ name: 'catalog.products.view', params: { id: id.value } })
  } else {
    router.push({ name: 'catalog.products.list' })
  }
}

function toggleEdit() {
  router.push({ name: 'catalog.products.edit', params: { id: id.value } })
}

onMounted(() => {
  load()
})
</script>

<template>
  <div>
    <PageHeader :title="title">
      <template #actions>
        <button
          v-if="mode === 'view'"
          class="p-button p-component"
          @click="toggleEdit"
        >
          Edit
        </button>
      </template>
    </PageHeader>

    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="error" :message="error" @retry="load" />

    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="formErrors.name" required>
            <input
              v-model="form.name"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Slug" :error="formErrors.slug" required>
            <input
              v-model="form.slug"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Status">
            <select
              v-model="form.status"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            >
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Archived">Archived</option>
            </select>
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Style Code">
            <input
              v-model="form.styleCode"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Department">
            <input
              v-model="form.department"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Gender Target">
            <input
              v-model="form.genderTarget"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-12">
          <FormField label="Description">
            <textarea
              v-model="form.description"
              rows="4"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :saving="saving"
        :save-label="mode === 'create' ? 'Create Product' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/pages/ProductDetailPage.vue
git commit -m "feat: implement ProductDetailPage with create/view/edit modes"
```

---

### Task 10: Build TaxonomyListPage

**Files:**
- Create: `app/Admin/src/features/catalog/pages/TaxonomyListPage.vue`

**Interfaces:**
- Consumes: `taxonomiesApi` from `../api`, `TaxonomyResponse` from models, shared components
- Produces: working taxonomy list with create button and row navigation to detail page

- [ ] **Step 1: Write TaxonomyListPage**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getTaxonomies, deleteTaxonomy } from '../api/taxonomies'
import type { TaxonomyResponse } from '../models/Taxonomy'

const router = useRouter()
const confirm = useConfirm()
const toast = useToastNotify()

const items = ref<TaxonomyResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'presentation', header: 'Presentation' },
  { field: 'position', header: 'Position' },
  { field: 'createdAt', header: 'Created' },
]

async function fetchTaxonomies() {
  loading.value = true
  error.value = null
  const result = await getTaxonomies({ page: page.value, pageSize: pageSize.value, search: search.value || undefined })
  if (result.success) {
    items.value = result.data
    totalCount.value = result.meta?.totalCount ?? 0
  } else {
    error.value = result.error.message ?? 'Failed to load taxonomies'
  }
  loading.value = false
}

function goToCreate() {
  router.push({ name: 'catalog.taxonomies.create' })
}

function goToView(id: string) {
  router.push({ name: 'catalog.taxonomies.view', params: { id } })
}

function goToEdit(id: string) {
  router.push({ name: 'catalog.taxonomies.edit', params: { id } })
}

async function onDelete(id: string) {
  await confirm({
    target: 'this taxonomy',
    onAccept: async () => {
      const result = await deleteTaxonomy(id)
      if (result.success) {
        toast.success('Taxonomy deleted')
        await fetchTaxonomies()
      } else {
        toast.error(result.error.message ?? 'Failed to delete')
      }
    },
  })
}

function onSearch() {
  page.value = 1
  fetchTaxonomies()
}

function onPageChange(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  fetchTaxonomies()
}

onMounted(() => fetchTaxonomies())
</script>

<template>
  <div>
    <PageHeader title="Taxonomies" subtitle="Manage taxonomy groups" />
    <TableToolbar
      v-model:search="search"
      search-placeholder="Search taxonomies..."
      create-label="Add Taxonomy"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :message="error" @retry="fetchTaxonomies" />
    <EmptyState
      v-else-if="items.length === 0"
      title="No taxonomies"
      description="Create your first taxonomy to organize categories."
    >
      <button @click="goToCreate">Add Taxonomy</button>
    </EmptyState>
    <DataTable
      v-else
      :value="items"
      :columns="columns"
      :loading="loading"
      :total-records="totalCount"
      :rows="pageSize"
      :first="(page - 1) * pageSize"
      lazy
      paginator
      striped-rows
      @page="onPageChange"
      @row-click="(e: { data: TaxonomyResponse }) => goToView(e.data.id)"
    >
      <template #body-actions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: 'Edit', icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/pages/TaxonomyListPage.vue
git commit -m "feat: implement TaxonomyListPage"
```

---

### Task 11: Build TaxonomyDetailPage with inline Taxons sub-table

**Files:**
- Create: `app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue`

**Interfaces:**
- Consumes: `taxonomiesApi`, `TaxonomyResponse`, `TaxonResponse`, `TaxonRequest` from models, shared components
- Produces: taxonomy detail page with create/view/edit modes + inline taxons sub-table with depth indentation

- [ ] **Step 1: Write TaxonomyDetailPage**

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import {
  getTaxonomy,
  createTaxonomy,
  updateTaxonomy,
  getTaxons,
  createTaxon,
  updateTaxon,
  deleteTaxon,
} from '../api/taxonomies'
import type { TaxonomyResponse, TaxonomyRequest, TaxonResponse, TaxonRequest } from '../models/Taxonomy'

const route = useRoute()
const router = useRouter()
const toast = useToastNotify()
const confirm = useConfirm()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const form = ref<TaxonomyRequest>({ name: '', presentation: null, position: 0 })
const formErrors = ref<Record<string, string>>({})

const taxons = ref<TaxonResponse[]>([])
const taxonsLoading = ref(false)
const taxonsError = ref<string | null>(null)
const showTaxonDrawer = ref(false)
const taxonForm = ref<TaxonRequest>({ name: '', presentation: null, parentId: null })
const taxonFormErrors = ref<Record<string, string>>({})
const editingTaxonId = ref<string | null>(null)
const taxonSaving = ref(false)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Taxonomy'
  if (mode.value === 'edit') return `Edit: ${form.value.name || 'Taxonomy'}`
  return form.value.name || 'Taxonomy Detail'
})

function validate(): boolean {
  formErrors.value = {}
  if (!form.value.name.trim()) formErrors.value.name = 'Required'
  return Object.keys(formErrors.value).length === 0
}

async function load() {
  if (!id.value) return
  loading.value = true
  error.value = null
  const result = await getTaxonomy(id.value)
  if (result.success) {
    form.value = { name: result.data.name, presentation: result.data.presentation, position: result.data.position }
    await loadTaxons()
  } else {
    error.value = result.error.message ?? 'Failed to load taxonomy'
  }
  loading.value = false
}

async function loadTaxons() {
  if (!id.value) return
  taxonsLoading.value = true
  taxonsError.value = null
  const result = await getTaxons(id.value)
  if (result.success) {
    taxons.value = result.data.sort((a, b) => a.lft - b.lft)
  } else {
    taxonsError.value = result.error.message ?? 'Failed to load taxons'
  }
  taxonsLoading.value = false
}

async function save() {
  if (!validate()) return
  saving.value = true
  const result = id.value
    ? await updateTaxonomy(id.value, form.value)
    : await createTaxonomy(form.value)
  saving.value = false
  if (result.success) {
    toast.success(id.value ? 'Taxonomy updated' : 'Taxonomy created')
    if (mode.value === 'create') {
      router.replace({ name: 'catalog.taxonomies.view', params: { id: result.data.id } })
    } else {
      await load()
      router.replace({ name: 'catalog.taxonomies.view', params: { id: id.value } })
    }
  } else {
    toast.error(result.error.message ?? 'Save failed')
  }
}

function cancel() {
  if (id.value) router.push({ name: 'catalog.taxonomies.view', params: { id: id.value } })
  else router.push({ name: 'catalog.taxonomies.list' })
}

function toggleEdit() {
  router.push({ name: 'catalog.taxonomies.edit', params: { id: id.value } })
}

function openTaxonDrawer(taxon?: TaxonResponse) {
  editingTaxonId.value = taxon?.id ?? null
  taxonForm.value = {
    name: taxon?.name ?? '',
    presentation: taxon?.presentation ?? null,
    parentId: taxon?.parentId ?? null,
  }
  taxonFormErrors.value = {}
  showTaxonDrawer.value = true
}

function validateTaxon(): boolean {
  taxonFormErrors.value = {}
  if (!taxonForm.value.name.trim()) taxonFormErrors.value.name = 'Required'
  return Object.keys(taxonFormErrors.value).length === 0
}

async function saveTaxon() {
  if (!validateTaxon() || !id.value) return
  taxonSaving.value = true
  const result = editingTaxonId.value
    ? await updateTaxon(id.value, editingTaxonId.value, taxonForm.value)
    : await createTaxon(id.value, taxonForm.value)
  taxonSaving.value = false
  if (result.success) {
    toast.success(editingTaxonId.value ? 'Taxon updated' : 'Taxon created')
    showTaxonDrawer.value = false
    await loadTaxons()
  } else {
    toast.error(result.error.message ?? 'Save failed')
  }
}

async function onDeleteTaxon(taxonId: string) {
  if (!id.value) return
  await confirm({
    target: 'this taxon',
    onAccept: async () => {
      const result = await deleteTaxon(id.value!, taxonId)
      if (result.success) {
        toast.success('Taxon deleted')
        await loadTaxons()
      } else {
        toast.error(result.error.message ?? 'Failed to delete')
      }
    },
  })
}

const taxonColumns = [
  { field: 'name', header: 'Name' },
  { field: 'slug', header: 'Slug' },
  { field: 'depth', header: 'Depth' },
  { field: 'position', header: 'Position' },
]

onMounted(() => load())
</script>

<template>
  <div>
    <PageHeader :title="title">
      <template #actions>
        <button
          v-if="mode === 'view'"
          class="p-button p-component"
          @click="toggleEdit"
        >
          Edit
        </button>
      </template>
    </PageHeader>

    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="4" :columns="2" />
    <ErrorState v-else-if="error" :message="error" @retry="load" />

    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="formErrors.name" required>
            <input
              v-model="form.name"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Presentation">
            <input
              v-model="form.presentation"
              type="text"
              class="p-inputtext p-component w-full"
              :disabled="mode === 'view'"
            />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :saving="saving"
        :save-label="mode === 'create' ? 'Create Taxonomy' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />

      <!-- Taxons sub-table (only in view/edit mode) -->
      <div v-if="id" class="mt-4">
        <fieldset class="p-fieldset p-component">
          <legend class="p-fieldset-legend">Taxons</legend>
          <TableToolbar
            v-model:search="(v: string) => {}"
            search-placeholder="Filter taxons..."
            create-label="Add Taxon"
            :hide-search="true"
            @create="openTaxonDrawer()"
          />
          <LoadingSkeleton v-if="taxonsLoading" :rows="4" :columns="4" />
          <ErrorState v-else-if="taxonsError" :message="taxonsError" @retry="loadTaxons" />
          <EmptyState
            v-else-if="taxons.length === 0"
            title="No taxons"
            description="Add categories to this taxonomy."
          >
            <button @click="openTaxonDrawer()">Add Taxon</button>
          </EmptyState>
          <DataTable v-else :value="taxons" :columns="taxonColumns">
            <template #body-name="{ data }">
              <span :style="{ paddingLeft: data.depth * 1.5 + 'rem' }">
                {{ data.name }}
              </span>
            </template>
            <template #body-actions="{ data }">
              <ActionMenu
                :items="[
                  { label: 'Edit', icon: 'pi pi-pencil', command: () => openTaxonDrawer(data) },
                  { label: 'Delete', icon: 'pi pi-trash', command: () => onDeleteTaxon(data.id) },
                ]"
              />
            </template>
          </DataTable>
        </fieldset>
      </div>
    </div>

    <!-- Taxon slideover drawer -->
    <div v-if="showTaxonDrawer" class="p-drawer p-component p-drawer-right" style="width: 400px">
      <div class="p-drawer-header">
        <h3>{{ editingTaxonId ? 'Edit Taxon' : 'Add Taxon' }}</h3>
      </div>
      <div class="p-drawer-content p-4">
        <FormField label="Name" :error="taxonFormErrors.name" required>
          <input v-model="taxonForm.name" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <FormField label="Presentation">
          <input v-model="taxonForm.presentation" type="text" class="p-inputtext p-component w-full" />
        </FormField>
      </div>
      <div class="p-drawer-footer p-3 flex justify-content-end gap-2">
        <button class="p-button p-component p-button-secondary" @click="showTaxonDrawer = false">Cancel</button>
        <button class="p-button p-component" :disabled="taxonSaving" @click="saveTaxon">
          {{ taxonSaving ? 'Saving...' : 'Save' }}
        </button>
      </div>
    </div>
    <div v-if="showTaxonDrawer" class="p-drawer-mask" @click="showTaxonDrawer = false" />
  </div>
</template>
```

- [ ] **Step 2: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue
git commit -m "feat: implement TaxonomyDetailPage with inline taxons sub-table"
```

---

### Task 12: Build OptionTypeListPage and OptionTypeDetailPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/OptionTypeListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue`

**Interfaces:**
- Consumes: `optionTypesApi`, `OptionTypeResponse`, `OptionValueResponse` from models, shared components
- Produces: option type list + detail page with inline option values sub-table

**Note:** These follow the same pattern as TaxonomyListPage + TaxonomyDetailPage.
Reuse the template from Tasks 10-11, replacing:
- `taxonomy` → `optionType`, `taxon` → `optionValue`
- API functions from `../api/optionTypes`
- Types from `../models/OptionType`
- Route names: `catalog.option-types.list`, `.create`, `.view`, `.edit`
- Table columns: Name, Presentation, Filterable, Position

- [ ] **Step 1: Write OptionTypeListPage** — follow TaxonomyListPage pattern

<details>
<summary>OptionTypeListPage.vue (same structure as TaxonomyListPage, click to expand)</summary>

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getOptionTypes, deleteOptionType } from '../api/optionTypes'
import type { OptionTypeResponse } from '../models/OptionType'

const router = useRouter()
const confirm = useConfirm()
const toast = useToastNotify()

const items = ref<OptionTypeResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'presentation', header: 'Presentation' },
  { field: 'filterable', header: 'Filterable' },
  { field: 'position', header: 'Position' },
]

async function fetch() {
  loading.value = true
  error.value = null
  const result = await getOptionTypes({ page: page.value, pageSize: pageSize.value, search: search.value || undefined })
  if (result.success) {
    items.value = result.data
    totalCount.value = result.meta?.totalCount ?? 0
  } else {
    error.value = result.error.message ?? 'Failed to load option types'
  }
  loading.value = false
}

function goToCreate() { router.push({ name: 'catalog.option-types.create' }) }
function goToView(id: string) { router.push({ name: 'catalog.option-types.view', params: { id } }) }
function goToEdit(id: string) { router.push({ name: 'catalog.option-types.edit', params: { id } }) }

async function onDelete(id: string) {
  await confirm({
    target: 'this option type',
    onAccept: async () => {
      const result = await deleteOptionType(id)
      if (result.success) { toast.success('Option type deleted'); await fetch() }
      else { toast.error(result.error.message ?? 'Failed to delete') }
    },
  })
}

function onSearch() { page.value = 1; fetch() }
function onPageChange(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  fetch()
}

onMounted(() => fetch())
</script>

<template>
  <div>
    <PageHeader title="Option Types" subtitle="Manage product option types (Size, Color, etc.)" />
    <TableToolbar
      v-model:search="search"
      search-placeholder="Search option types..."
      create-label="Add Type"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :message="error" @retry="fetch" />
    <EmptyState v-else-if="items.length === 0" title="No option types" description="Create your first option type.">
      <button @click="goToCreate">Add Type</button>
    </EmptyState>
    <DataTable
      v-else
      :value="items"
      :columns="columns"
      :loading="loading"
      :total-records="totalCount"
      :rows="pageSize"
      :first="(page - 1) * pageSize"
      lazy paginator striped-rows
      @page="onPageChange"
      @row-click="(e: { data: OptionTypeResponse }) => goToView(e.data.id)"
    >
      <template #body-filterable="{ data }">
        <span>{{ data.filterable ? 'Yes' : 'No' }}</span>
      </template>
      <template #body-actions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: 'Edit', icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
```

</details>

- [ ] **Step 2: Write OptionTypeDetailPage** — follow TaxonomyDetailPage pattern, replacing taxons with option values

The pattern is identical to Task 11 (TaxonomyDetailPage) with these substitutions:
- `getTaxonomy` → `getOptionType`, `createTaxonomy` → `createOptionType`, `updateTaxonomy` → `updateOptionType`
- `getTaxons` → `getOptionValues`, `createTaxon` → `createOptionValue`, `updateTaxon` → `updateOptionValue`, `deleteTaxon` → `deleteOptionValue`
- Types: `TaxonomyResponse` → `OptionTypeResponse`, `TaxonResponse` → `OptionValueResponse`
- Route names: `catalog.taxonomies.*` → `catalog.option-types.*`
- Fieldset legend: "Taxons" → "Option Values"
- No depth indentation on option values (flat list)
- OptionValue fields: Name, Presentation, Position (no parentId, no slug, no depth)

Write the complete `OptionTypeDetailPage.vue` — same structure as TaxonomyDetailPage (Task 11) with these exact substitutions:
- Imports: `optionTypesApi` → `../api/optionTypes` (`getOptionType`, `createOptionType`, `updateOptionType`, `getOptionValues`, `createOptionValue`, `updateOptionValue`, `deleteOptionValue`)
- Types: `TaxonomyResponse` → `OptionTypeResponse`, `TaxonomyRequest` → `OptionTypeRequest`, `TaxonResponse` → `OptionValueResponse`, `TaxonRequest` → `OptionValueRequest` (all from `../models/OptionType`)
- Route names: `catalog.taxonomies.*` → `catalog.option-types.*`
- `form` initial value: `{ name: '', presentation: null, position: 0, filterable: false }` (add `filterable` field)
- `validate()`: add `filterable` field to form
- Form template: add `filterable` checkbox field; remove fields not on OptionType (slug, description, status, etc.)
- Sub-table Fieldset `legend`: "Taxons" → "Option Values"
- Sub-table entity: `taxons` → `optionValues`, `taxonForm` → `optionValueForm`, `editingTaxonId` → `editingOptionValueId`
- Open drawer: remove `parentId` from `optionValueForm`; no `parentId` on OptionValue entity
- Sub-table columns: `['name', 'presentation', 'position']` (no `slug`, no `depth`)
- Sub-table body-name: remove depth-based indent padding (OptionValues are flat)
- `loadTaxons()` → `loadOptionValues()`, variable renames accordingly
- Title computed: "Taxonomy" → "Option Type", "taxon" → "option value" in toast messages

Full file (~200 lines). See Task 11 for the template structure.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/pages/OptionTypeListPage.vue
git add app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue
git commit -m "feat: implement OptionTypeListPage and OptionTypeDetailPage"
```

---

### Task 13: Update Catalog DashboardPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/DashboardPage.vue`

**Interfaces:**
- Consumes: shared components
- Produces: dashboard with StatCard grid for catalog KPIs

- [ ] **Step 1: Write Catalog DashboardPage**

```vue
<script setup lang="ts">
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StatCard from '@/shared/components/data/StatCard.vue'

const stats = [
  { label: 'Total Products', value: '0', icon: 'pi pi-shopping-bag', color: 'primary' as const },
  { label: 'Active Products', value: '0', icon: 'pi pi-check-circle', color: 'green' as const },
  { label: 'Taxonomies', value: '0', icon: 'pi pi-sitemap', color: 'blue' as const },
  { label: 'Option Types', value: '0', icon: 'pi pi-list', color: 'orange' as const },
]
</script>

<template>
  <div>
    <PageHeader title="Catalog Dashboard" subtitle="Overview of your product catalog" />
    <div class="grid">
      <div v-for="s in stats" :key="s.label" class="col-12 md:col-6 lg:col-3">
        <StatCard :label="s.label" :value="s.value" :icon="s.icon" :color="s.color" />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/pages/DashboardPage.vue
git commit -m "feat: implement Catalog DashboardPage with KPI cards"
```

---

### Task 14: Verify full Catalog module

- [ ] **Step 1: Run typecheck**

```bash
cd app/Admin && pnpm run typecheck
# Expected: no errors
```

- [ ] **Step 2: Run lint**

```bash
cd app/Admin && pnpm run lint
# Expected: no errors
```

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && pnpm run test:unit
# Expected: all tests pass
```

- [ ] **Step 4: Run dev server and verify navigation**

```bash
cd app/Admin && pnpm run dev
# Navigate to Catalog > Products — verify list loads
# Navigate to Catalog > All Taxonomies — verify list loads
# Navigate to Catalog > All Types — verify list loads
```

- [ ] **Step 5: Commit**

```bash
git add -A app/Admin/src/
git commit -m "chore: final verification — typecheck, lint, tests pass"
```

---

## Self-Review Checklist

After completing all tasks, verify:

1. All 8 removed page files no longer exist
2. All 9 modules have `api/`, `components/`, `composables/`, `models/`, `pages/`, `store/`, `utils/` directories
3. All route files reference correct page component paths
4. Menu config has no stale route references
5. Catalog module: 7 pages = Dashboard, ProductList, ProductDetail, TaxonomyList, TaxonomyDetail, OptionTypeList, OptionTypeDetail
6. TaxonomyDetailPage taxons sub-table uses depth-based CSS indentation
7. OptionTypeDetailPage option values sub-table is flat (no indentation)
8. DetailPage mode detection works: `/new` = create, `/:id` = view, `/:id/edit` = edit
